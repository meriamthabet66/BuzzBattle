using System.Collections.Generic;

namespace Data {
    [System.Serializable]
    public class CharacterData
    {
        public long id;
        public string nickname;
        public string skin_url;
        public List<ItemData> Items;
        
        public System.Collections.Generic.List<string> EquippedItemNames = new();

    }
}