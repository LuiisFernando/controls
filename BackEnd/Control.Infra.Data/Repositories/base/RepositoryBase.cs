﻿using Control.Domain.Interfaces.Repositories;
using Control.Infra.Data.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Control.Infra.Data.Repositories
{
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
    {
        protected readonly ControlContext Context;

        public RepositoryBase(ControlContext context)
        {
            this.Context = context;
        }

        protected DbSet<TEntity> DbSet
        {
            get
            {
                return Context.Set<TEntity>();
            }
        }

        #region 'Comandos de Execução'

        public async Task<int> ExecuteSqlAsync(string query)
        {
            return await Context.Database.ExecuteSqlCommandAsync(query);
        }

        #endregion

        #region 'Comandos de CRUD'

        public TEntity Insert(TEntity model)
        {
            DbSet.Add(model);
            this.Save();
            return model;
        }

        public async Task<TEntity> InsertAsync(TEntity model)
        {
            DbSet.Add(model);
            await this.SaveAsync();
            return model;
        }

        public bool Update(TEntity model)
        {
            var entry = Context.Entry(model);

            DbSet.Attach(model);

            entry.State = EntityState.Modified;

            return (this.Save() > 0);
        }

        public async Task<bool> UpdateAsync(TEntity model)
        {
            var entry = Context.Entry(model);

            DbSet.Attach(model);

            entry.State = EntityState.Modified;

            return (await this.SaveAsync() > 0);
        }

        public bool Delete(TEntity model)
        {
            var entry = Context.Entry(model);

            DbSet.Attach(model);

            entry.State = EntityState.Deleted;

            return (this.Save() > 0);
        }

        public async Task<bool> DeleteAsync(TEntity model)
        {
            var entry = Context.Entry(model);

            DbSet.Attach(model);

            entry.State = EntityState.Deleted;

            return (await this.SaveAsync() > 0);
        }

        public bool Delete(Expression<Func<TEntity, bool>> where)
        {
            var model = DbSet.Where<TEntity>(where).FirstOrDefault<TEntity>();

            return (model != null) && Delete(model);
        }

        public async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> where)
        {
            var model = DbSet.Where<TEntity>(where).FirstOrDefault<TEntity>();

            return (model != null) && await DeleteAsync(model);
        }

        public bool Delete(params object[] Keys)
        {
            var model = DbSet.Find(Keys);
            return (model != null) && Delete(model);
        }

        public async Task<bool> DeleteAsync(params object[] Keys)
        {
            var model = DbSet.Find(Keys);
            return (model != null) && await DeleteAsync(model);
        }


        #endregion

        #region 'Comandos de Busca'

        public TEntity Find(params object[] Keys)
        {
            return DbSet.Find(Keys);
        }

        public async Task<TEntity> FindAsync(params object[] Keys)
        {
            return await DbSet.FindAsync(Keys);
        }

        public TEntity Find(Expression<Func<TEntity, bool>> where)
        {
            return DbSet.Where<TEntity>(where).FirstOrDefault<TEntity>();
        }

        public TEntity Find(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            if (includeProperties != null && includeProperties.Length != 0)
            {
                var q = DbSet.Include(includeProperties.First());

                foreach (var property in includeProperties.Skip(1))
                {
                    q = q.Include(property);
                }

                return q.SingleOrDefault(predicate);
            }

            return DbSet.SingleOrDefault(predicate);
        }

        public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            if (includeProperties != null && includeProperties.Length != 0)
            {
                var q = DbSet.Include(includeProperties.First());

                foreach (var property in includeProperties.Skip(1))
                {
                    q = q.Include(property);
                }

                return q.Where(predicate).AsQueryable();
            }

            return DbSet.Where(predicate).AsQueryable();
        }

        public IEnumerable<TEntity> FindAll(Expression<Func<TEntity, bool>> where)
        {
            return DbSet.Where<TEntity>(where);
        }

        public async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> where)
        {
            return await DbSet.Where<TEntity>(where).AsNoTracking().FirstOrDefaultAsync<TEntity>();
        }

        public IQueryable<TEntity> Get()
        {
            return DbSet;
        }

        public IQueryable<TEntity> Get(params Expression<Func<TEntity, object>>[] includes)
        {

            return includes.Aggregate(this.Get(),
                (current, expression) => current.Include(expression));
        }

        #endregion

        #region 'Comandos Genéricos'

        public int Save()
        {
            return this.Context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await this.Context.SaveChangesAsync();
        }

        #endregion
    }
}
