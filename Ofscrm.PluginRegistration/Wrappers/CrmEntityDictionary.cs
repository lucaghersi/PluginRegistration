using System;
using System.Collections;
using System.Collections.Generic;

namespace Ofscrm.PluginRegistration.Wrappers
{
    public class CrmEntityDictionary<EntityType> : IEnumerable<EntityType>, IEnumerable
        where EntityType : ICrmEntity
    {
        #region Private Fields

        private Dictionary<Guid, EntityType> m_entityList = new Dictionary<Guid, EntityType>();

        #endregion Private Fields

        #region Public Constructors

        public CrmEntityDictionary(Dictionary<Guid, EntityType> entityList)
        {
            if (entityList == null)
            {
                throw new ArgumentNullException("entityList");
            }

            m_entityList = entityList;
        }

        #endregion Public Constructors

        #region Public Properties

        public int Count
        {
            get
            {
                return m_entityList.Count;
            }
        }

        public Dictionary<Guid, EntityType>.KeyCollection Keys
        {
            get
            {
                return m_entityList.Keys;
            }
        }

        public Dictionary<Guid, EntityType>.ValueCollection Values
        {
            get
            {
                return m_entityList.Values;
            }
        }

        #endregion Public Properties

        #region Public Indexers

        public EntityType this[Guid id]
        {
            get
            {
                return m_entityList[id];
            }
        }

        #endregion Public Indexers

        #region Public Methods

        public bool ContainsKey(Guid id)
        {
            return m_entityList.ContainsKey(id);
        }

        public IEnumerator<EntityType> GetEnumerator()
        {
            return m_entityList.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_entityList.Values.GetEnumerator();
        }

        public EntityType[] ToArray()
        {
            EntityType[] items = new EntityType[m_entityList.Count];
            m_entityList.Values.CopyTo(items, 0);

            return items;
        }

        public bool TryGetValue(Guid id, out EntityType value)
        {
            return m_entityList.TryGetValue(id, out value);
        }

        #endregion Public Methods
    }
}