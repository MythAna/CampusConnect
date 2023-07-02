using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
           return await _context.Users
           .Where(x => x.UserName == username)
           .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
           .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await _context.Users
             .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
             .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int Id)
        {
            return await _context.Users.FindAsync(Id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        Task<object> IUserRepository.GetUserAsync()
        {
            throw new NotImplementedException();
        }

        Task<AppUser> IUserRepository.GetUserByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        Task<AppUser> IUserRepository.GetUserByUsernameAsync(string username)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<AppUser>> IUserRepository.GetUsersAsync()
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserRepository.SaveAllAsync()
        {
            throw new NotImplementedException();
        }

        void IUserRepository.Update(AppUser user)
        {
            throw new NotImplementedException();
        }
    }
}