﻿using nvxapp.server.data.Entities;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace nvxapp.server.data.Repositories
{
    public class AspNetUsersRepository : Repository<ApplicationDbContext, ApplicationUser>, IAspNetUsersRepository
    {
        public AspNetUsersRepository(ApplicationDbContext dbContext, IServiceProvider provider) : base(dbContext, provider)
        {
        }
    }
    public interface IAspNetUsersRepository : IRepository<ApplicationUser>
    {

    }
}
