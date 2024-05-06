using System.Collections.Generic;

namespace DRC.RTS.Resources
{
    public class StorageManager
    {
        private readonly GatheringBag<Resources.GameResources.EResourceType, int> ResourcesStorage = new();

        public void StoreResources(GatheringBag<GameResources.EResourceType, int> Resources)
        {
            foreach (KeyValuePair<GameResources.EResourceType, int> entry in Resources)
            {
                ResourcesStorage.AddOrUpdate(entry.Key, entry.Value);
            }
        }

        public GatheringBag<Resources.GameResources.EResourceType, int> GetResources()
        {
            return ResourcesStorage;
        }
    }
}
