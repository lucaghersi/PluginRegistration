﻿using System;
using Microsoft.Xrm.Sdk;

namespace Ofscrm.PluginRegistration.Entities.Transformation
{
    public static class Magic
    {
        #region Public Methods

        public static T CastTo<T>(Entity entity)
            where T : Entity
        {
            var instance = (T)Activator.CreateInstance(typeof(T));

            if (instance != null)
            {
                instance.Attributes = entity.Attributes;
            }

            return instance;
        }

        #endregion Public Methods
    }
}