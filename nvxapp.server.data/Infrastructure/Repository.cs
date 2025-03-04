using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using nvxapp.server.data.Extensions;
using nvxapp.server.data.Interfaces;
using Serilog;
using System.Linq.Expressions;
using System.Reflection;

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

        private string? _currentSchema;
        public string? CurrentSchema
        {
            get { return _currentSchema; }
            set { _currentSchema = value; }
        }

        /* questa prop verra utilizzara nelle classi derivate che implementano 
           ICurrentTenant che assegna il tenant in base al login
           E' stata messa qui, per metterla a disposizione di tutti i repo 
           che dovranno leggere i dati si schemi diversi da bublic
        */
        private string? _currentTenant;
        public string? CurrentTenant
        {
            get { return _currentTenant; }
            set
            {
                _currentTenant = value;
                this.CurrentSchema = _currentTenant;
            }
        }


        //protected string CurrentTenat
        //{
        //    get
        //    {
        //        if (_httpContextAccessor.HttpContext != null)
        //        {
        //            var tenant = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant")?.Value;
        //            return tenant ?? "public";
        //        }

        //        return "public";
        //    }
        //}

        public Repository(TDbContext context,
                          IServiceProvider serviceProvider,
                          IHttpContextAccessor httpContextAccessor)
        {
            DbContext = context;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<T> UpsertAsync(T entity)
        {

            try
            {
                Schema_Set();



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
            finally
            {
                Schema_resume();
            }

        }

        public async Task<T> CreateAsync(T entity, Boolean SaveChanges = true)
        {

            try
            {
                Schema_Set();

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

                throw;
            }
            finally
            {
                Schema_resume();
            }



            return entity;
        }

        public async Task<T> UpsertAsyncGuid(T entity)
        {

            try
            {
                Schema_Set();


                PropertyInfo? prop = null;
                string id = "";

                prop = entity.GetType().GetProperty("Id");
                if (prop != null)
                    id = (string)(prop.GetValue(entity) ?? "");
                else
                {
                    prop = entity.GetType().GetProperty("id");
                    if (prop != null)
                        id = (string)(prop.GetValue(entity) ?? "");
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
            finally
            {
                Schema_resume();
            }
        }

        public async Task DeleteAsync(T entity)
        {
            try
            {
                Schema_Set();

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
            finally
            {
                Schema_resume();
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                Schema_Set();

                DbContext.Set<T>().RemoveRange(entities);

                await DbContext.SaveChangesAsync();

            }
            finally
            {
                Schema_resume();
            }
        }

        public DbSet<T> GetAll()
        {
            

            try
            {
                Schema_Set();
                return DbContext.Set<T>();
            }
            finally
            {
                Schema_resume();
            }
        }

        public async Task<List<T>> FindAll()
        {
            try
            {
                Schema_Set();
                return await DbContext.Set<T>().ToListAsync();
            }
            finally
            {
                Schema_resume();
            }



        }

        public IQueryable<T> FindAll(Expression<Func<T, bool>> where)
        {
            try
            {
                Schema_Set();
                return DbContext.Set<T>().Where(where);
            }
            finally
            {
                Schema_resume();
            }


        }

        public T? FindById(int id)
        {
            try
            {
                Schema_Set();
                return DbContext.Set<T>().Find(id);
            }
            finally
            {
                Schema_resume();
            }
            
        }

        public async Task<T?> FindByIdAsync(int id)
        {
            try
            {
                Schema_Set();
                return await DbContext.Set<T>().FindAsync(id);
            }
            finally
            {
                Schema_resume();
            }
            
        }

        public async Task<T?> FindByIdAsync(int id1, int id2)
        {
            
            try
            {
                Schema_Set();
                return await DbContext.Set<T>().FindAsync(id1, id2);
            }
            finally
            {
                Schema_resume();
            }
        }

        public async Task<T?> FindByIdAsync(int id1, int id2, int id3)
        {
            try
            {
                Schema_Set();
                return await DbContext.Set<T>().FindAsync(id1, id2, id3);
            }
            finally
            {
                Schema_resume();
            }
            
        }

        public async Task<T> UpdateAsync(T entity)
        {

            try
            {
                Schema_Set();

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

                throw;
            }
            finally
            {
                Schema_resume();
            }



            return entity;
        }

        public async Task SaveChange()
        {


            try
            {
                Schema_Set();
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Log.Error(ex.InnerException.Message);
                else
                    Log.Error(ex.Message);

                throw;
            }
            finally
            {
                Schema_resume();
            }


        }

        private void Schema_Set()
        {
            SharedSchema.CurrentSchema = CurrentSchema ?? "public";
        }
        private void Schema_resume()
        {
            SharedSchema.CurrentSchema = "public";
        }


    }

}
