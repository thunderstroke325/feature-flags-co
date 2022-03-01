using FeatureFlags.APIs.Authentication;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.APIs.Services
{
    public interface IUserInvitationService 
    {
        public Task ClearAsync(string userId);
    }

    public class UserInvitationService : IUserInvitationService
    {
        private readonly ApplicationDbContext _dbContext;

        public UserInvitationService(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public async Task ClearAsync(string userId)
        {
            var invitations = await _dbContext.UserInvitations
                .Where(x => x.UserId == userId)
                .ToListAsync();

            _dbContext.UserInvitations.RemoveRange(invitations);

            await _dbContext.SaveChangesAsync();
        }
    }
}
