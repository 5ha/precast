namespace PrecastTracker.Data;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
