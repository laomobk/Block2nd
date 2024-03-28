using System.Collections.Generic;
using Block2nd.GameSave;
using UnityEngine;

namespace Block2nd.GUI
{
    public class WorldSelectUI : MonoBehaviour
    {
        public GameObject worldPreviewItemPrefab;

        public void ClearAllItems()
        {
            int n = transform.childCount;

            for (int i = 0; i < n; ++i)
            {
                DestroyImmediate(transform.GetChild(i));
            }
        }

        public void SetPreviewList(List<LevelSavePreview> previews)
        {
            ClearAllItems();
            
            for (int i = 0; i < previews.Count; ++i)
            {
                AppendItem(previews[i], i);
            }
        }

        private void AppendItem(LevelSavePreview preview, int idx)
        {
            var go = Instantiate(worldPreviewItemPrefab, transform);
            var item = go.GetComponent<WorldPreviewItem>();

            var rect = item.GetComponent<RectTransform>();

            rect.anchoredPosition = new Vector2(0, idx * 75);
            
            item.Setup(preview);
        }
    }
}