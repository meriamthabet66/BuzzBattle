using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Questions {
    [CreateAssetMenu(menuName = "Questions/Category Database")]
    
    //this is the database of all the categories, it has a list that contains all of them and some logic to apply on that list
    public class CategoryDatabase : ScriptableObject
    {
        public List<Category> categories;
        
        public Category GetCategoryByName(string name)
        {
            return categories.Find(c => c.categoryName == name);
        }

        public Category GetRandomCategory()
        {
            if (categories.Count == 0) return null;
            return categories[Random.Range(0, categories.Count)];
        }
    }
}