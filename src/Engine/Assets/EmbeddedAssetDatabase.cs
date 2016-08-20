﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Veldrid.Assets;

namespace Engine.Assets
{
    public class EmbeddedAssetDatabase : AssetDatabase
    {
        private static readonly Type _lazyGenericType = typeof(Lazy<>);

        private readonly Dictionary<AssetID, object> _assets = new Dictionary<AssetID, object>();

        public void RegisterAsset<T>(AssetID id, T asset)
        {
            _assets.Add(id, asset);
        }

        public void RegisterAsset<T>(AssetID id, Lazy<T> asset)
        {
            _assets.Add(id, asset);
        }

        public T LoadAsset<T>(AssetRef<T> assetRef)
        {
            return LoadAsset<T>(assetRef.ID);
        }

        public T LoadAsset<T>(AssetID assetID)
        {
            return MaterializeAsset<T>(_assets[assetID]);
        }

        public object LoadAsset(AssetID assetID)
        {
            return MaterializeAsset<object>(_assets[assetID]);
        }

        public AssetID[] GetAssetsOfType(Type t)
        {
            List<AssetID> ids = new List<AssetID>();
            foreach (var kvp in _assets)
            {
                Type assetType = MaterializeAssetType(kvp.Value);
                if (t.IsAssignableFrom(assetType))
                {
                    ids.Add(kvp.Key);
                }
            }

            return ids.ToArray();
        }

        public bool TryLoadAsset<T>(AssetID id, out T asset)
        {
            object assetAsObject;
            if (_assets.TryGetValue(id, out assetAsObject))
            {
                asset = MaterializeAsset<T>(assetAsObject);
                return true;
            }
            else
            {
                asset = default(T);
                return false;
            }
        }

        private T MaterializeAsset<T>(object asset)
        {
            var valueType = asset.GetType();
            if (valueType.IsConstructedGenericType && valueType.GetGenericTypeDefinition() == _lazyGenericType)
            {
                var itemType = valueType.GenericTypeArguments[0];
                if (itemType == typeof(T))
                {
                    return ((Lazy<T>)asset).Value;
                }
                if (!typeof(T).IsAssignableFrom(itemType))
                {
                    throw new InvalidOperationException("Asset type mismatch. Desired: " + typeof(T).Name + ", Actual: " + itemType.Name);
                }
                else
                {
                    var property = valueType.GetProperty("Value");
                    return (T)property.GetValue(asset);
                }
            }
            else
            {
                return (T)asset;
            }
        }

        private Type MaterializeAssetType(object value)
        {
            var valueType = value.GetType();
            if (valueType.IsConstructedGenericType && valueType.GetGenericTypeDefinition() == _lazyGenericType)
            {
                return valueType.GenericTypeArguments[0];
            }
            else
            {
                return value.GetType();
            }
        }
    }
}
