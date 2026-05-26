namespace Data {
    [System.Serializable]
    public class ItemData {
        public long id;
        public string item_name;
        public string item_type;
        public string item_image_url;
        public int price;
        public bool is_locked; // Pulled from the ownership_registry link
    }
}