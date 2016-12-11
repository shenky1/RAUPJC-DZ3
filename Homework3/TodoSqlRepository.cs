using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework3
{
    public class TodoSqlRepository : ITodoRepository
    {

        private readonly TodoDbContext _context;

        public TodoSqlRepository(TodoDbContext context)
        {
            _context = context;
        }

        public void Add(TodoItem todoItem)
        {
            if (todoItem == null)
            {
                throw new ArgumentNullException();
            }
            if (_context.TodoItems.Any(t => t.Id.Equals(todoItem.Id)))
            {
                throw new DuplicateTodoItemException("duplicate id: {" + todoItem.Id.ToString() + "}");
            }

            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();
        }

        public TodoItem Get(Guid todoId, Guid userId)
        {
            TodoItem item = _context.TodoItems.FirstOrDefault(s => s.Id == todoId);
            if (item == null)
            {
                return null;
            }
            if (item.UserId == userId)
            {
                return item;
            }
            throw new TodoAccessDeniedException("Given user does not own given item.");
        }

        public List<TodoItem> GetActive(Guid userId)
        {
            return _context.TodoItems.Where(t => t.UserId.Equals(userId) && (t.IsCompleted == false))
                .OrderByDescending(t => t.DateCreated).ToList();
        }

        public List<TodoItem> GetCompleted(Guid userId)
        {
            return _context.TodoItems.Where(t => t.UserId.Equals(userId) && t.IsCompleted)
                .OrderByDescending(t => t.DateCreated).ToList();
        }

        public List<TodoItem> GetFiltered(Func<TodoItem, bool> filterFunction, Guid userId)
        {
            if (filterFunction == null) throw new ArgumentNullException();
            return _context.TodoItems.Where(filterFunction)
                .Where(t => t.UserId.Equals(userId))
                .OrderByDescending(t => t.DateCreated).ToList();
        }

        public List<TodoItem> GetAll(Guid userId)
        {
            return _context.TodoItems.Where(t => t.UserId.Equals(userId)).OrderByDescending(t => t.DateCreated).ToList();
        }

        public bool MarkAsCompleted(Guid todoId, Guid userId)
        {
            if (!_context.TodoItems.Any(t => t.Id.Equals(todoId)))
            {
                return false;
            }
            Get(todoId, userId).MarkAsCompleted();
            _context.SaveChanges();
            return true;
        }

        public bool Remove(Guid todoId, Guid userId)
        {
            TodoItem item = Get(todoId, userId);
            if (item == null)
            {
                return false;
            }
            _context.TodoItems.Remove(item);
            _context.SaveChanges();
            return true;
        }

        public void Update(TodoItem todoItem, Guid userId)
        {
            if (todoItem == null)
            {
                throw new ArgumentNullException();
            }
            TodoItem item = Get(todoItem.Id, userId);

            if (item != null)
            {
                item.Text = todoItem.Text;
                item.DateCompleted = todoItem.DateCompleted;
                item.DateCompleted = todoItem.DateCreated;
                item.IsCompleted = todoItem.IsCompleted;
            }

            else
                Add(todoItem);
        }
    }
}
