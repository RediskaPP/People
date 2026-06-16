using People.Models;
using SQLite;

namespace People;

public class PersonRepository
{
    string _dbPath;
    public string StatusMessage { get; set; }
    private SQLiteAsyncConnection conn;
    private async Task Init()
    {
        if (conn != null)
            return;
        conn = new SQLiteAsyncConnection(_dbPath);
        await conn.CreateTableAsync<Person>();
    }

    public PersonRepository(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task AddNewPerson(string name, string email = "", string phone = "")
    {
        try
        {
            await Init();

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Valid name required");

            var person = new Person
            {
                Name = name.Trim(),
                Email = email?.Trim() ?? string.Empty,
                Phone = phone?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            int result = await conn.InsertAsync(person);
            StatusMessage = $"{result} record(s) added [Name: {name}]";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to add {name}. Error: {ex.Message}";
        }
    }

    public async Task<List<Person>> GetAllPeople()
    {
        try
        {
            await Init();
            return await conn.Table<Person>()
                             .OrderBy(p => p.Name)
                             .ToListAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to retrieve data. {ex.Message}";
            return new List<Person>();
        }
    }

    public async Task<Person?> GetPersonById(int id)
    {
        try
        {
            await Init();
            return await conn.Table<Person>()
                             .Where(p => p.Id == id)
                             .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to get person #{id}. {ex.Message}";
            return null;
        }
    }

    public async Task<List<Person>> SearchPeople(string query)
    {
        try
        {
            await Init();

            if (string.IsNullOrWhiteSpace(query))
                return await GetAllPeople();

            string q = query.ToLowerInvariant();

            return await conn.Table<Person>()
                             .Where(p => p.Name.ToLower().Contains(q)
                                      || p.Email.ToLower().Contains(q)
                                      || p.Phone.Contains(q))
                             .OrderBy(p => p.Name)
                             .ToListAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search failed. {ex.Message}";
            return new List<Person>();
        }
    }

    public async Task<int> GetPeopleCount()
    {
        try
        {
            await Init();
            return await conn.Table<Person>().CountAsync();
        }
        catch
        {
            return 0;
        }
    }

    public async Task UpdatePerson(Person person)
    {
        try
        {
            await Init();

            if (string.IsNullOrWhiteSpace(person.Name))
                throw new Exception("Valid name required");

            person.Name = person.Name.Trim();
            person.Email = person.Email?.Trim() ?? string.Empty;
            person.Phone = person.Phone?.Trim() ?? string.Empty;

            int result = await conn.UpdateAsync(person);
            StatusMessage = result > 0
                ? $"Person #{person.Id} updated successfully"
                : $"Person #{person.Id} not found";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to update person #{person.Id}. Error: {ex.Message}";
        }
    }

    public async Task DeletePerson(int id)
    {
        try
        {
            await Init();
            int result = await conn.DeleteAsync<Person>(id);
            StatusMessage = result > 0
                ? $"Person #{id} deleted"
                : $"Person #{id} not found";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to delete person #{id}. Error: {ex.Message}";
        }
    }

    public async Task DeleteAllPeople()
    {
        try
        {
            await Init();
            int result = await conn.DeleteAllAsync<Person>();
            StatusMessage = $"{result} record(s) deleted";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to clear table. Error: {ex.Message}";
        }
    }
}
