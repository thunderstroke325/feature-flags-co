namespace FeatureFlags.APIs.Services.MongoDb
{
    /// <summary>
    /// Defines an entity. 
    /// Use <see cref="IEntity{TKey}"/> where possible for better integration to repositories and other structures.
    /// </summary>
    public interface IEntity
    {
        
    }

    /// <summary>
    /// Defines an entity with a single primary key with "Id" property.
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
    public interface IEntity<TKey> : IEntity
    {
        /// <summary>
        /// Unique identifier for this entity.
        /// </summary>
        TKey Id { get; }
    }
}