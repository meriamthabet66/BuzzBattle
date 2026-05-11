using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using GamePlay.Questions;
using Data.DTO;
using System.Linq;
using System.IO;
using GamePlay.Systems;
using Newtonsoft.Json; // Ensure you have Newtonsoft installed via NuGet!

namespace Managers {
    public class CategoryCloudManager : MonoBehaviour {
        public static CategoryCloudManager Instance { get; private set; }

        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // --- FETCHING & DOWNLOADING ---

        public async Task<List<Category>> GetCategoryList() {
            var response = await SupabaseManager.Instance.Client.From<CategoryDTO>().Get();
            List<Category> list = new List<Category>();
            foreach (var dto in response.Models) {
                Category cat = ScriptableObject.CreateInstance<Category>();
                cat.id = dto.id;
                cat.categoryName = dto.category_name;
                cat.price = dto.price;
                list.Add(cat);
            }
            return list;
        }

        public async Task DownloadQuestions(Category cat) {
            var client = SupabaseManager.Instance.Client;

            var mcqTask = client.From<MCQQuestionDTO>().Where(x => x.category_id == cat.id).Get();
            var tfTask = client.From<TFQuestionDTO>().Where(x => x.category_id == cat.id).Get();
            var vTask = client.From<VerbalQuestionDTO>().Where(x => x.category_id == cat.id).Get();
            await Task.WhenAll(mcqTask, tfTask, vTask);

            List<long> allIds = new List<long>();
            allIds.AddRange(mcqTask.Result.Models.Select(m => m.id));
            allIds.AddRange(tfTask.Result.Models.Select(t => t.id));
            allIds.AddRange(vTask.Result.Models.Select(v => v.id));

            var baseResponse = await client.From<BaseQuestionDTO>().Filter("id", Postgrest.Constants.Operator.In, allIds).Get();
            var baseLookup = baseResponse.Models.ToDictionary(b => b.id, b => b);

            cat.multipleChoiceQuestions.Clear();
            foreach (var mDto in mcqTask.Result.Models) {
                var options = await client.From<MCQOptionDTO>().Where(o => o.mcq_id == mDto.id).Get();
                cat.multipleChoiceQuestions.Add(QuestionFactory.CreateMCQ(baseLookup[mDto.id], options.Models));
            }

            cat.trueOrFalseQuestions.Clear();
            foreach (var tDto in tfTask.Result.Models) {
                cat.trueOrFalseQuestions.Add(QuestionFactory.CreateTF(baseLookup[tDto.id], tDto));
            }

            cat.verbalQuestions.Clear();
            foreach (var vDto in vTask.Result.Models) {
                cat.verbalQuestions.Add(QuestionFactory.CreateVerbal(baseLookup[vDto.id], vDto));
            }
        }

        // --- OFFLINE SAVE / LOAD LOGIC ---

        public void SaveCategoryLocally(Category cat) {
            CategorySavePack pack = new CategorySavePack {
                categoryId = cat.id,
                categoryName = cat.categoryName,
                mcqs = cat.multipleChoiceQuestions.Select(q => new MCQSaveData {
                    baseData = new BaseQuestionDTO { id = q.id, question_text = q.questionText, difficulty = q.difficulty },
                    options = q.answers.Select(a => new MCQOptionDTO { answer_text = a.text, is_correct = a.isCorrect }).ToList()
                }).ToList(),
                tfs = cat.trueOrFalseQuestions.Select(q => new TFSaveData {
                    baseData = new BaseQuestionDTO { id = q.id, question_text = q.questionText, difficulty = q.difficulty },
                    tfData = new TFQuestionDTO { id = q.id, is_true = q.isTrue }
                }).ToList(),
                verbals = cat.verbalQuestions.Select(q => new VerbalSaveData {
                    baseData = new BaseQuestionDTO { id = q.id, question_text = q.questionText, difficulty = q.difficulty },
                    vData = new VerbalQuestionDTO { id = q.id, correct_answer_text = q.correctAnswer }
                }).ToList()
            };

            string json = JsonConvert.SerializeObject(pack);
            string path = Path.Combine(Application.persistentDataPath, $"cat_{cat.id}.json");
            File.WriteAllText(path, json);
            Debug.Log($"<color=cyan>Offline Pack Saved: {path}</color>");
        }
        
      

        public List<Category> GetLocalDownloadedCategories() 
        {
            List<Category> localList = new List<Category>();
            string folderPath = Application.persistentDataPath;
    
            // Look for all files starting with "cat_" and ending in ".json"
            string[] files = Directory.GetFiles(folderPath, "cat_*.json");

            foreach (string file in files) 
            {
                try {
                    string json = File.ReadAllText(file);
                    CategorySavePack pack = JsonConvert.DeserializeObject<CategorySavePack>(json);
            
                    // Reconstruct a temporary ScriptableObject for the UI to show
                    Category cat = ScriptableObject.CreateInstance<Category>();
                    cat.id = pack.categoryId;
                    cat.categoryName = pack.categoryName;
                    // Note: In a real game, you'd save the Icon path too
            
                    localList.Add(cat);
                } catch { /* Skip corrupted files */ }
            }
            return localList;
        }

        public void LoadCategoryFromDisk(Category targetSO) {
            if (targetSO == null) return;

            string path = Path.Combine(Application.persistentDataPath, $"cat_{targetSO.id}.json");
            if (!File.Exists(path)) {
                Debug.LogWarning($"No offline file found for {targetSO.categoryName} at {path}");
                return;
            }

            string json = File.ReadAllText(path);
            CategorySavePack pack = JsonConvert.DeserializeObject<CategorySavePack>(json);

            if (pack == null) {
                Debug.LogError("Failed to read JSON data from disk.");
                return;
            }

            // --- CRITICAL SAFETY: Initialize lists if they are null ---
            if (targetSO.multipleChoiceQuestions == null) targetSO.multipleChoiceQuestions = new List<MultipleChoiceQuestion>();
            if (targetSO.trueOrFalseQuestions == null) targetSO.trueOrFalseQuestions = new List<TrueOrFalseQuestion>();
            if (targetSO.verbalQuestions == null) targetSO.verbalQuestions = new List<VerbalQuestion>();

            // Clear old data safely
            targetSO.multipleChoiceQuestions.Clear();
            targetSO.trueOrFalseQuestions.Clear();
            targetSO.verbalQuestions.Clear();

            // Populate MCQ
            if (pack.mcqs != null) {
                foreach (var m in pack.mcqs) 
                    targetSO.multipleChoiceQuestions.Add(QuestionFactory.CreateMCQ(m.baseData, m.options));
            }

            // Populate TF
            if (pack.tfs != null) {
                foreach (var t in pack.tfs) 
                    targetSO.trueOrFalseQuestions.Add(QuestionFactory.CreateTF(t.baseData, t.tfData));
            }

            // Populate Verbal
            if (pack.verbals != null) {
                foreach (var v in pack.verbals) 
                    targetSO.verbalQuestions.Add(QuestionFactory.CreateVerbal(v.baseData, v.vData));
            }

            Debug.Log($"<color=green>SUCCESS: Loaded {targetSO.categoryName} from Disk. Count: {targetSO.multipleChoiceQuestions.Count} MCQs.</color>");
        }

        public bool IsCategoryDownloaded(long id) {
            return File.Exists(Path.Combine(Application.persistentDataPath, $"cat_{id}.json"));
        }
    }

    // --- DATA PACKS FOR STORAGE ---
    [System.Serializable]
    public class CategorySavePack {
        public long categoryId;
        public string categoryName;
        public List<MCQSaveData> mcqs;
        public List<TFSaveData> tfs;
        public List<VerbalSaveData> verbals;
    }

    [System.Serializable] public class MCQSaveData { public BaseQuestionDTO baseData; public List<MCQOptionDTO> options; }
    [System.Serializable] public class TFSaveData { public BaseQuestionDTO baseData; public TFQuestionDTO tfData; }
    [System.Serializable] public class VerbalSaveData { public BaseQuestionDTO baseData; public VerbalQuestionDTO vData; }
}