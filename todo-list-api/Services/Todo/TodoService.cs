using TodoApi.Models;
using TodoApi.Service.ITodoService;
using MongoDB.Driver;
using MongoDB.Bson;
using TodoApi.Data;

namespace TodoApi.Service.ITodoService
{
    public class TodoService : ITodoService
    {
        private readonly IMongoCollection<TodoItemModel> _todos;
        private readonly IMongoCollection<CategoryItemModel> _categories;


        public TodoService(MongoDbContext context)
        {
            _todos = context.Database.GetCollection<TodoItemModel>("TodoItems");
            _categories = context.Database.GetCollection<CategoryItemModel>("CategoryItems");
        }

        public List<TodoItemModel> GetAllTodo()
        {
            return _todos.Find(new BsonDocument()).ToList();
        }

        public TodoItemModel GetTodo(string id)
        {
            // Retrieve the corresponding todo
            var filter = Builders<TodoItemModel>.Filter.Eq("_id", ObjectId.Parse(id));
            return _todos.Find(filter).FirstOrDefault();
        }

        public TodoItemModel CreateTodo(TodoItemModel todo)
        {
            _todos.InsertOne(todo);

            // Retrieve the corresponding category
            var categoryFilter = Builders<CategoryItemModel>.Filter.Eq("_id", ObjectId.Parse(todo.CategoryId));
            var category = _categories.Find(categoryFilter).FirstOrDefault();

            if (category != null)
            {
                category.TodoItems.Add(todo);

                // Update the category in the Category collection
                var updateDefinition = Builders<CategoryItemModel>.Update.Set(c => c.TodoItems, category.TodoItems);
                _categories.UpdateOne(categoryFilter, updateDefinition);
            }

            return todo;
        }


        public void UpdateTodo(string id, UpdateDefinition<TodoItemModel> updateDefinition)
        {
            // Retrieve the corresponding todo
            var filter = Builders<TodoItemModel>.Filter.Eq("_id", ObjectId.Parse(id));
            _todos.UpdateOne(filter, updateDefinition);
        }

        public void RemoveTodo(string id)
        {
            // Retrieve the corresponding todo
            var filter = Builders<TodoItemModel>.Filter.Eq("_id", ObjectId.Parse(id));
            _todos.DeleteOne(filter);
        }

        public async Task<CategoryItemModel> GetCategory(string categoryId)
        {
            var category = await _categories.Find(x => x.Id == categoryId).FirstOrDefaultAsync();

            if (category != null)
            {
                // Load Todo items associated with this category
                category.TodoItems = await _todos.Find(x => x.CategoryId == categoryId).ToListAsync();
            }

            return category;
        }

        public async Task<bool> RemoveCategoryAsync(string categoryId)
        {
            // Delete category
            var categoryDeleteResult = await _categories.DeleteOneAsync(x => x.Id == categoryId);

            if (categoryDeleteResult.DeletedCount == 0)
            {
                return false;
            }

            // Deletes all items associated with the category
            await _todos.DeleteManyAsync(x => x.CategoryId == categoryId);

            return true;
        }


    }
}
