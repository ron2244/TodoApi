using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todoapi.Models;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Http;
using System.Text.Json;
namespace Todoapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class peopleController : ControllerBase
    {
        private readonly TodoContext _context;
    

        public peopleController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/people
        [HttpGet]
        public async Task<ActionResult<IEnumerable<people>>> Getpeoples()
        {


            return await _context.peoples.ToListAsync();
        }

        // GET: api/people/5
        [HttpGet("{id}")]
        public async Task<ActionResult<people>> Getpeople(string id)
        {
            var people = await _context.peoples.FindAsync(id);

            if (people == null)
            {
                return NotFound("A person with the id '"+id+"' does not exist.");
            }

            return people;
        }
        // GET: api/people/{id}/tasks
        [HttpGet("{id}/tasks/{status?}")]
        public async Task<ActionResult<task>> GetpeopleTasks(string id, state status=state.na)
        {
            var people = await _context.peoples.FindAsync(id);

            if (people == null)
            {
                return NotFound("A person with the id '"+id+"' does not exist.");
            }
            //search the database for tasks of this people with status "status"
            List<task> list;
            if (status == state.na) //If optional parameter status wasn't given
                list =  await _context.tasks.Where(x => x.ownerId == id).ToListAsync();
            else
                list =  await _context.tasks.Where(x => (x.ownerId == id) && (x.status==status)).ToListAsync();    
            
            return CreatedAtAction(nameof(GetpeopleTasks), new { id = people.Id }, list);
        }
        

        // PUT: api/people/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putpeople(string id, people people)
        {
            if (id != people.Id)
            {
                return BadRequest();
            }

            _context.Entry(people).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!peopleExists(id))
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

        // POST: api/people
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<people>> Postpeople(people people)
        {
            people.activeTaskCount=0;
            var counter = 0;
            if (_context.peoples.Any())
                counter = Int32.Parse(_context.peoples.Max(x=>x.Id)) + 1;
            people.Id = counter.ToString();
            _context.peoples.Add(people);
            if (peopleEmailExists(people.email)){
                return BadRequest("A person with email '"+people.email+"' already exists.");
            }
                 
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest();
            }
            HttpContext.Response.Headers.Add("x-Created-Id", people.Id);
            return CreatedAtAction(nameof(Getpeople), new { id = people.Id }, people);
        }

        // POST: api/people/{id}/tasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{id}/tasks")]
        public async Task<ActionResult<people>> PostpeopleTask(string id,task task)
        {
            if (task.status == null)
                task.status = state.active;
            task.ownerId = id;
            var counter = 0;
            if (_context.tasks.Any())
                counter = Int32.Parse(_context.tasks.Max(x=>x.Id)) + 1;
            task.Id = counter.ToString();
            _context.tasks.Add(task);
            if (!peopleExists(id))
            {
                return NotFound("A person with the id '"+id+"' does not exist.");
            }
            if (task.status == state.active){ //If the task status is active, update the owner's task count
                people people =  await _context.peoples.FindAsync(id);
                people.activeTaskCount = people.activeTaskCount+1;
            } 
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
          
                    throw;
            
            }

            HttpContext.Response.Headers.Add("x-Created-Id", task.Id);
            return CreatedAtAction(nameof(PostpeopleTask), new { id = id }, task);
        }

        private async void updateTasks(string id){
            var people =  await _context.peoples.FindAsync(id);
            people.activeTaskCount++;
        }

        // DELETE: api/people/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletepeople(string id)
        {
            var people = await _context.peoples.FindAsync(id);
            if (people == null)
            {
                return NotFound("A person with the id '"+id+"' does not exist.");
            }

            _context.peoples.Remove(people);
            //Delete all the tasks related to the deleted person
            foreach (task t in _context.tasks){
                if (t.ownerId == id)
                    _context.tasks.Remove(t);
            }
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool peopleExists(string id)
        {
            return _context.peoples.Any(e => e.Id == id);
        }

        private bool peopleEmailExists(string email)
        {
            return _context.peoples.Any(e => e.email == email);
        }

        
    [HttpPatch("{id}")]
    public IActionResult Patch(string id, [FromBody] JsonPatchDocument<people> patchEntity)
    {
        var entity = _context.peoples.FirstOrDefault(people => people.Id == id);
 
        if (entity == null)
        {
            return NotFound("A person with the id '"+id+"' does not exist.");
        }
        foreach (var op in patchEntity.Operations){
            if (op.path == "/email"){ //If the operation is to change the owner email, check if email exists
                if (peopleEmailExists(op.value.ToString()))
                    return BadRequest("A person with email '"+op.value.ToString()+"' already exists.");
            } //Cannot edit active task count
            if (op.path == "/activeTaskCount") 
                return BadRequest("ActiveTaskCount cannot be changed");
        }
                    
        patchEntity.ApplyTo(entity, ModelState); // Must have Microsoft.AspNetCore.Mvc.NewtonsoftJson installed
        _context.SaveChanges();
        return Ok(entity);
    }
    }


}
