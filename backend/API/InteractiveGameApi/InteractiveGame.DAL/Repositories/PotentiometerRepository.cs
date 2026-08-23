using InteractiveGameApi.InteractiveGame.DAL.Entities;

namespace InteractiveGameApi.InteractiveGame.DAL.Repositories
{
    public class PotentiometerRepository
    {
        private readonly InteractiveGameDbContext _context;

        public PotentiometerRepository(InteractiveGameDbContext context)
        {
            _context = context;
        }

        public async Task SaveResultAsync(PotentiometerResult result)
        {
            _context.PotentiometerResults.Add(result);
            await _context.SaveChangesAsync();
        }
    }

}
