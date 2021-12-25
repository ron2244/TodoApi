using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using Todoapi.Models;
using System.IO;
using System.Web.Http.Cors;

namespace Todoapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class tasksController : ControllerBase
    {
        private readonly TodoContext _context;

        public tasksController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<task>>> Gettasks()
        {
            return await _context.tasks.ToListAsync();
        }

        // GET: api/tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<task>> Gettask(string id)
        {
            var task = await _context.tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound("A task with the id '"+id+"' does not exist.");
            }

            return task;
        }

                // GET: api/tasks/5/status
        [HttpGet("{id}/status")]
        public async Task<ActionResult<string>> GettaskStatus(string id)
        {
            var task = await _context.tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound("A task with the id '"+id+"' does not exist.");
            }

            return Enum.GetName(typeof(state),task.status);
        }
        // GET: api/tasks/5/owner
        [HttpGet("{id}/owner")]
        public async Task<ActionResult<string>> GettaskOwner(string id)
        {
            var task = await _context.tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound("A task with the id '"+id+"' does not exist.");
            }

            return task.ownerId;
        }

        // PUT: api/tasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Puttask(string id, task task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!taskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/tasks/5/status
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/status")]
        public async Task<IActionResult> PuttaskStatus(string id)
        {
            var task = await _context.tasks.FindAsync(id);
            if (task ==null)
            {
                return NotFound("A task with the id '"+id+"' does not exist.");
            }
            string body="";
            //Read the request body
            using (StreamReader stream = new StreamReader(Request.Body))
            {         
                body = await stream.ReadToEndAsync();
            }
            //Check if input is legal status
            if (body != "active" & body!="done")
                return BadRequest("value '"+body+"' is not a legal task status.");
            people person = _context.peoples.Find(task.ownerId);
            string current_state = task.status.ToString();
            //Check if the change is from done to active, then increment active task count
            if (body == "active" & current_state!=body)
                person.activeTaskCount++;
            else if (current_state!=body)
                    person.activeTaskCount--;  //Check if the change is from active to done
            //Cast the input string to the Enum "state" value
            task.status = (state)Enum.Parse(typeof(state),body);    
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!taskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        
        // PUT: api/tasks/5/owner/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/owner")]
        public async Task<IActionResult> PuttaskOwner(string id)
        {
            var task = await _context.tasks.FindAsync(id);
            if (task ==null )
            {
                return NotFound("A task with the id '"+id+"' does not exist.");
            }
            string body="";
            //Read the request body
            using (StreamReader stream = new StreamReader(Request.Body))
            {         
                body = await stream.ReadToEndAsync();
            }
            //Fetch the current and new owners of the task
            people new_person = await _context.peoples.FindAsync(body);
            people old_person = await _context.peoples.FindAsync(task.ownerId);
            if (new_person == null)
                return BadRequest("A person with the id '"+body+"' does not exists.");
            if (task.status == state.active){  //If the task is active, update the active task count for both of them
                    new_person.activeTaskCount++;
                    old_person.activeTaskCount--;
            }
            task.ownerId = body;        
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!taskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/tasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<task>> Posttask(task task)
        {
            _context.tasks.Add(task);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (taskExists(task.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("Gettask", new { id = task.Id }, task);
        }

        // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletetask(string id)
        {
            var task = await _context.tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound("A task with the id '"+id+"' does not exist.");
            }

            _context.tasks.Remove(task);
            //If the task is active, update the owner's active task count
            if (task.status == state.active)
                _context.peoples.Find(task.ownerId).activeTaskCount--;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool taskExists(string id)
        {
            return _context.tasks.Any(e => e.Id == id);
        }

    [HttpPatch("{id}")]
    public IActionResult Patch(string id, [FromBody] JsonPatchDocument<task> patchEntity)
    {
        var entity = _context.tasks.FirstOrDefault(task => task.Id == id);

        if (entity == null )
        {
            return NotFound("A task with the id '"+id+"' does not exist.");
        }
        state status = state.na;
        var person_id = "";
        //Go through the patch operations from the input
        foreach (var op in patchEntity.Operations){
            if (op.path == "/ownerId"){ //If the operation is to change the owner ID
                if (_context.peoples.Find(op.value) == null)
                    return NotFound("A person with the id '"+op.value+"' does not exist.");
                person_id = op.value.ToString();
            } //If the operation is to change the status and the status is different from current status
            if (op.path == "/status" && (state)Enum.Parse(typeof(state),op.value.ToString()) != entity.status) 
                status = (state)Enum.Parse(typeof(state),op.value.ToString());
        }
        
        //If the status was changed, update the active task count accordingly
        if (status != state.na){
            if (person_id == ""){
                people person = _context.peoples.Find(entity.ownerId);
                if (status == state.active)
                    person.activeTaskCount++;
                else
                    person.activeTaskCount--; 
            }
            else {
                    // Change the ownerId if needed
                people new_person = _context.peoples.Find(person_id);
                people old_person = _context.peoples.Find(entity.ownerId);
                if (status == state.active){
                    new_person.activeTaskCount++;
                }
                else
                    old_person.activeTaskCount--;
            
            }      
        } //Only change the ownerId, not the status
        else if (person_id != "" && entity.status== state.active){
            _context.peoples.Find(entity.ownerId).activeTaskCount--;
            _context.peoples.Find(person_id).activeTaskCount++;
        }

        patchEntity.ApplyTo(entity, ModelState); // Must have Microsoft.AspNetCore.Mvc.NewtonsoftJson installed
        _context.SaveChanges();
        return Ok(entity);
    }
    }
}
