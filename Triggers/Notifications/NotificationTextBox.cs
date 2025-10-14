using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Triggers.Notifications
{
    public abstract class NotificationTextBox
    {
        public Rectangle Rect {  get; set; }
        public Keys Key { get; set; }
        public List<TextData> TextData { get; set; } = new List<TextData>();
        public IProximityTracked AnchorPoint {  get; set; }
        public Vector2 CacheAnchorPoint { get; set; }
        public bool Active { get; set; } = false;
        public float FadeInTimer { get; set; }
        public float FadeTimerMax { get; set; }
        public float FadeTimerCurrent {  get; set; }
        public float BoxLifeTimer { get; set; } = 0f;
        public SpriteFont Font { get; set; }
        public const int Padding = 10;
        public const float MaxTextWidth = 100f;

        public string WrappedText { get; set; }


        public void MarkInactive()
        {
            Active = false;
        }
        public void MarkActive()
        {
            ResetCurrentFadeTimer();
            Active = true;
        }
        public void ResetCurrentFadeTimer()
        {
            FadeTimerCurrent = FadeTimerMax;
        }
        public abstract void GetTypeSpecificDrawPoints();
        public void SetTexts(List<string> texts)
        {
            TextData.Clear();

            for (int i = 0; i < texts.Count; i++)
            {
                float baseAngle = (i % 2 == 0) ? 350f : 10f;
                float reduction = i * 5f;
                float finalAngle = (i % 2 == 0)
                    ? baseAngle + reduction 
                    : baseAngle - reduction; 

                TextData.Add(new TextData
                {
                    Text = texts[i],
                    FadeDelay = i * 2f,
                    Rotation = MathHelper.ToRadians(finalAngle)
                });
            }
        }
        public void ClearLifeTimeTimer()
        {
            BoxLifeTimer = 0;
        }
        public virtual void SetCacheAnchorPoint()
        {
            CacheAnchorPoint = AnchorPoint.ProximityTrackingPoint;
        }
        public void UpdateLifeTimeTimers(float delta)
        {
            BoxLifeTimer += delta;
        }

        internal virtual bool AnchorPointMoved(Vector2 anchorPoint)
        {
            return CacheAnchorPoint == anchorPoint;
        }
    }
    public class CombatNotificationTextBox : NotificationTextBox
    {
        public CombatNotificationTextBox(IProximityTracked proxy)
        {
            AnchorPoint = proxy;
            TextData.Add(new TextData
            {
                Text = "Press E to Engage",
                FadeDelay = 0
            });
            Key = Keys.E;
            FadeTimerMax = 1f;
            FadeInTimer = 2f;
            Font = AssetManager.GetFont("mainFont");
        }
        public override void GetTypeSpecificDrawPoints()
        {
            Vector2 anchorPoint = CacheAnchorPoint;
            foreach (var text in TextData)
            {
                text.DrawPoint  = anchorPoint +new Vector2(0, 32f);
            }
        }

    }
    public class MessageNotificationTextBox : NotificationTextBox
        
    {
        public MessageNotificationTextBox(IProximityTracked proxy, NotificationTextBoxData data)
        {
            AnchorPoint = proxy;
            FadeTimerMax = 2f;
            FadeInTimer = 2f;
            Font = AssetManager.GetFont("NotificationBoxFont");
            SetTexts(data.Texts);
        }

        public override void GetTypeSpecificDrawPoints()
        {
            for (int i = 0; i < TextData.Count; i++)
            {
                float xOffset = (i % 2 == 0 ? -1 : 1) * (128f - i * 28f);
                float yOffset = -96f + (i * 12f); 

               

                TextData[i].DrawPoint = CacheAnchorPoint + new Vector2(xOffset, yOffset);
            }
        }


    }
    public class TextData
    {
        public string Text { get; set; }
        public float FadeDelay { get; set; }
        public Vector2 DrawPoint { get; set; }
        public float Rotation { get; set; }
    }

}
