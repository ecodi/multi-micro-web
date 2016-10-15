using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CandidateDocuments.Application.Models;
using CandidateDocuments.Application.Repositories;

namespace CandidateDocuments.Infrastructure.Repositories
{
    /// <summary>
    /// Entity Framework based repository.
    /// </summary>
    public class BaseModelRepository<TModel> : IBaseModelRepository<TModel> where TModel : BaseModel, new()
    {
        private readonly DbContext _context;

        public BaseModelRepository(DbContext context)
        {
            _context = context;
        }

        public virtual IEnumerable<TModel> AllIncluding(params Expression<Func<TModel, object>>[] includeProperties)
        {
            var query = _context.Set<TModel>().AsQueryable();
            foreach (var includeProperty in includeProperties)
                query = query.Include(includeProperty);
            return query.AsEnumerable();
        }

        public virtual IEnumerable<TModel> GetAll()
        {
            return _context.Set<TModel>().AsEnumerable();
        }

        public TModel GetSingle(Guid id)
        {
            return _context.Set<TModel>().FirstOrDefault(x => x.Id == id);
        }

        public virtual IEnumerable<TModel> FindBy(Expression<Func<TModel, bool>> predicate,
            Expression<Func<TModel, object>> orderExpression = null, int skip = 0, int take = 0)
        {
            var query = _context.Set<TModel>().Where(predicate).AsQueryable();
            if (orderExpression != null) query = query.OrderBy(orderExpression);
            if (skip > 0) query = query.Skip(skip);
            if (take > 0) query = query.Take(take);
            return query.AsEnumerable();
        }

        public virtual void Add(TModel entity)
        {
            _context.Entry(entity);
            _context.Set<TModel>().Add(entity);
        }

        public virtual void Update(TModel entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(TModel entity)
        {
            _context.Entry(entity).State = EntityState.Deleted;
        }

        public virtual void DeleteWhere(Expression<Func<TModel, bool>> predicate)
        {
            var entities = _context.Set<TModel>().Where(predicate);
            foreach (var entity in entities)
                Delete(entity);
        }

        public virtual void Commit()
        {
            _context.SaveChanges();
        }
    }
}
