
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Interactions
{
    public class PromotionButton : UdonSharpBehaviour
    {
        public PromotionPicker picker;
        public string promotionType;
        public Text availableCountLabel;

        /// <summary>
        /// Refresh the counter showing how many pieces of the <see cref="promotionType"/> are left to be spawned
        /// </summary>
        public void _RefreshCounter()
        {
            availableCountLabel.text = picker.board.GetPiecesAvailableCount(promotionType).ToString();
        }

        public override void Interact()
        {
            picker.PromoteTo(promotionType);
        }
    }

}
