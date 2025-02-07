using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using nvxapp.server.data.Extensions;
using nvxapp.server.data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace nvxapp.server.data.Infrastructure
{
    public class Repository<TDbContext, T> : ICurrentUser, IRepository<T> where TDbContext : DbContext where T : class
    {
        protected TDbContext DbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private string? _currentUser;
        public string? CurrentUserId
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        public Repository(TDbContext context, 
                          IServiceProvider serviceProvider,
                          IHttpContextAccessor httpContextAccessor)
        {
            DbContext = context;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        protected string? CurrentTenat
        {
            get
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var tenant = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant")?.Value;
                    return tenant;
                }

                return null;
            }
        }


        public async Task<T> UpsertAsync(T entity)
        {

            PropertyInfo? prop = null;
            int id = 0;

            prop = entity.GetType().GetProperty("Id");
            if (prop != null)
                id = (int)(prop.GetValue(entity) ?? 0);
            else
            {
                prop = entity.GetType().GetProperty("id");
                if (prop != null)
                    id = (int)(prop.GetValue(entity) ?? 0);
                else
                {
                    prop = entity.GetType().GetProperty("ID");
                    if (prop != null)
                        id = (int)(prop.GetValue(entity) ?? 0);
                }
            }

            //var id = prop == null ? 0 : (int)prop.GetValue(entity);




            var validator = _serviceProvider.GetService<IValidator<T>>();

            if (validator != null)
            {
                var validationResults = validator.Validate(entity).ToList();

                if (validationResults.Any())
                    throw new Exception(validationResults.ToStringMessage());
            }

            //// update the "ChangeDate" property if exists
            //var changeDateProp = entity.GetType().GetProperty("ModifiedDate");
            //if (changeDateProp != null) changeDateProp.SetValue(entity, DateTime.Now);

            return id == 0 ? await CreateAsync(entity) : await UpdateAsync(entity);

        }

        public async Task<T> CreateAsync(T entity, Boolean SaveChanges = true)
        {

            try
            {
                var now = DateTime.Now;

                // update the "ChangeDate" property if exists
                var changeDatePropUp = entity.GetType().GetProperty("ModifiedDate");
                if (changeDatePropUp != null) changeDatePropUp.SetValue(entity, now);

                // update the "CreationDate" property if exists
                var changeDateProp = entity.GetType().GetProperty("CreationDate");
                if (changeDateProp != null) changeDateProp.SetValue(entity, now);

                var changeUsrPropUp = entity.GetType().GetProperty("ChangeUser");
                if (changeUsrPropUp != null) changeUsrPropUp.SetValue(entity, CurrentUserId);

                DbContext.Set<T>().Add(entity);

                if (SaveChanges)
                    await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Log.Error(ex.InnerException.Message, entity);
                else
                    Log.Error(ex.Message, entity);

                throw ;
            }



            return entity;
        }

        public async Task<T> UpsertAsyncGuid(T entity)
        {

            PropertyInfo? prop = null;
            string id = "";

            prop = entity.GetType().GetProperty("Id");
            if (prop != null)
                id = (string)(prop.GetValue(entity) ?? "");
            else
            {
                prop = entity.GetType().GetProperty("id");
                if (prop != null)
                    id = (string)(prop.GetValue(entity)??"");
                else
                {
                    prop = entity.GetType().GetProperty("ID");
                    if (prop != null)
                        id = (string)(prop.GetValue(entity) ?? "");
                }
            }

            var validator = _serviceProvider.GetService<IValidator<T>>();

            if (validator != null)
            {
                var validationResults = validator.Validate(entity).ToList();

                if (validationResults.Any())
                    throw new Exception(validationResults.ToStringMessage());
            }

            //// update the "ChangeDate" property if exists
            //var changeDateProp = entity.GetType().GetProperty("ChangeDate");
            //if (changeDateProp != null) changeDateProp.SetValue(entity, DateTime.Now);

            return id == "" ? await CreateAsync(entity) : await UpdateAsync(entity);

        }


        public async Task DeleteAsync(T entity)
        {

            var validator = _serviceProvider.GetService<IValidator<T>>();

            if (validator != null)
            {
                var validationResults = validator.CanBeEliminated(entity).ToList();

                if (validationResults.Any())
                    throw new Exception(validationResults.ToStringMessage());
            }


            DbContext.Set<T>().Remove(entity);

            await DbContext.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            DbContext.Set<T>().RemoveRange(entities);

            await DbContext.SaveChangesAsync();
        }

        public DbSet<T> GetAll()
        {
            return DbContext.Set<T>();
        }

        public async Task<List<T>> FindAll()
        {
            return await DbContext.Set<T>().ToListAsync();
        }

        public IQueryable<T> FindAll(Expression<Func<T, bool>> where)
        {
            return DbContext.Set<T>().Where(where);
        }


        public T? FindById(int id)
        {
            return DbContext.Set<T>().Find(id);
        }

        public async Task<T?> FindByIdAsync(int id)
        {
            return await DbContext.Set<T>().FindAsync(id);
        }

        public async Task<T?> FindByIdAsync(int id1, int id2)
        {
            return await DbContext.Set<T>().FindAsync(id1, id2);
        }

        public async Task<T?> FindByIdAsync(int id1, int id2, int id3)
        {
            return await DbContext.Set<T>().FindAsync(id1, id2, id3);
        }

        public async Task<T> UpdateAsync(T entity)
        {

            try
            {

                var now = DateTime.Now;

                // update the "ChangeDate" property if exists
                var changeDatePropUp = entity.GetType().GetProperty("ModifiedDate");
                if (changeDatePropUp != null) changeDatePropUp.SetValue(entity, now);

                var changeUsrPropUp = entity.GetType().GetProperty("ChangeUser");
                if (changeUsrPropUp != null) changeUsrPropUp.SetValue(entity, CurrentUserId);




                DbContext.Set<T>().Update(entity);

                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Log.Error(ex.InnerException.Message, entity);
                else
                    Log.Error(ex.Message, entity);

                throw ;
            }



            return entity;
        }

        public async Task SaveChange()
        {


            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Log.Error(ex.InnerException.Message);
                else
                    Log.Error(ex.Message);

                throw ;
            }


        }

    }

}
