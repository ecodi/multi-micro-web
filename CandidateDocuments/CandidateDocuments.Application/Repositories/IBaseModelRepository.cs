using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CandidateDocuments.Application.Models;

namespace CandidateDocuments.Application.Repositories
{
    public interface IBaseModelRepository<TModel> where TModel : BaseModel, new()
    {
        /// <summary>
        /// Retrieves all available entities with eager loaded properties.
        /// </summary>
        /// <param name="includeProperties">Properties to eager load</param>
        /// <returns>Collection of entities</returns>
        IEnumerable<TModel> AllIncluding(params Expression<Func<TModel, object>>[] includeProperties);
        /// <summary>
        /// Retrieves all available entities.
        /// </summary>
        /// <returns>Collection of entities</returns>
        IEnumerable<TModel> GetAll();
        /// <summary>
        /// Searches for specific entity.
        /// </summary>
        /// <param name="id">ID of entity</param>
        /// <returns>Entity</returns>
        TModel GetSingle(Guid id);
        /// <summary>
        /// Retrives entities based on provided parameters.
        /// </summary>
        /// <param name="predicate">Filter method</param>
        /// <param name="orderExpression">Order method</param>
        /// <param name="skip">Number of skipped entities</param>
        /// <param name="take">Limit of returned entities</param>
        /// <returns>Collection of entities</returns>
        IEnumerable<TModel> FindBy(Expression<Func<TModel, bool>> predicate, Expression<Func<TModel, object>> orderExpression = null, int skip = 0, int take = 0);
        /// <summary>
        /// Adds entity to the context.
        /// </summary>
        /// <param name="entity">Entity</param>
        void Add(TModel entity);
        /// <summary>
        /// Updates entity in the context.
        /// </summary>
        /// <param name="entity">Entity</param>
        void Update(TModel entity);
        /// <summary>
        /// Deletes entity from the context.
        /// </summary>
        /// <param name="entity">Entity</param>
        void Delete(TModel entity);
        /// <summary>
        /// Deletes entities based on provided filter.
        /// </summary>
        /// <param name="predicate">Filter method</param>
        void DeleteWhere(Expression<Func<TModel, bool>> predicate);
        /// <summary>
        /// Saves changes made in context.
        /// </summary>
        void Commit();
    }
}
