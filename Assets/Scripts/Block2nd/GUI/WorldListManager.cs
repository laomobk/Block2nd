using System;
using System.Collections.Generic;
using Block2nd.GameSave;
using UnityEngine;

namespace Block2nd.GUI
{
    public class WorldListManager : MonoBehaviour
    {
        public GameObject worldPreviewItemPrefab;

        private LevelSavePreview current;
        private List<WorldPreviewItem> items = new List<WorldPreviewItem>();

        public Action<LevelSavePreview> onLevelSelectedAction;
        public Action<LevelSavePreview> onPlaySelectedWorldAction;

        public LevelSavePreview Current => current;

        public void ClearAllItems()
        {
            int n = transform.childCount;

            for (int i = 0; i < n; ++i)
            {
                DestroyImmediate(transform.GetChild(i));
            }
            
            items.Clear();
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

            rect.anchoredPosition = new Vector2(0, idx * -70);
            
            item.Setup(preview, this);
            items.Add(item);
        }

        public void OnItemSelect(WorldPreviewItem item)
        {
            foreach (var previewItem in items)
            {
                previewItem.SetHighlightState(false);
            }

            item.SetHighlightState(true);
            
            onLevelSelectedAction.Invoke(item.Preview);

            current = item.Preview;
        }

        public void OnPlaySelectedWorldClick()
        {
            onPlaySelectedWorldAction.Invoke(current);
        }
    }
}