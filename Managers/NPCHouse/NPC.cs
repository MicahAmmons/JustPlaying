using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Debug;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.Tiles;
using PlayingAround.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.Managers.NPCHouse
{
    public class NPC
    {
        public string name { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        private readonly NPCData _data;
        public DialogueData AllDialogue { get; set; }
        public Texture2D HeadTexture;
        public Color HeadColor;
        public Color EyeColor;
        public Texture2D LeftEyeTexture;
        public Texture2D RightEyeTexture;
        public Vector2 currentPos { get; set; }
        public Vector2 drawFromPosition { get; set; }
        public MovementController MovementController { get; set; }

        public NPC(NPCData data, Vector2 Pos, DialogueData dialogueData)
        {
            _data = data;
            currentPos = Pos;
            AllDialogue = dialogueData;
            name = data.name;
            MovementController = new MovementController(AnimationLibrary.GetAnimation(data.AnimationData), PlayingAround.Entities.Monster.CombatMonsters.CombatMonsterType.NPC);

            // width = data.width;
            //height = data.height;
            // HeadTexture = AssetManager.GetTexture(data.headTexturePath);
            // LeftEyeTexture = AssetManager.GetTexture("LeftFlat");
            // RightEyeTexture = AssetManager.GetTexture("RightOutward");
            // EyeColor = ColorPalette.GetElementColor( data.eyeColor);
            // HeadColor = ColorPalette.GetElementColor(data.headColor);
        }
        public void Draw(SpriteBatch sb)
        {
        }
        public void Update(GameTime gameTime)
        {
            MovementController.Update(gameTime);
        }
        public void DrawStaticFrames(SpriteBatch spriteBatch)
        {
            if (MovementController.AnimationManager.CurrentControllers == null) return;

            bool allAnimationIsFinished = MovementController.AnimationManager.IsFinished;
            foreach (var contr in MovementController.AnimationManager.CurrentControllers)
            {
                if (contr.Animation == null) continue;
                if (contr.IsFinished && !contr.Animation.IsLooping && !contr.Animation.HoldUntilAllFinished) continue;

                Animation animation = contr.Animation;
                bool flipHorizontal = MovementController.FlipHorizontally(animation.DefaultDirection);
                if (animation.RotatesTowardDirection) flipHorizontal = false;

                Vector2 drawPoint = contr.Animation.DrawPointOverride != null
                    ? (Vector2)contr.Animation.DrawPointOverride
                    : MovementController.DrawPoint;

                int width = animation.Width;
                int height = animation.Height;
                int yOffset = animation.YOffset;

                var pos = animation.OverrideDiamondDrawPoint
                    ? drawPoint
                    : TileManager.OffSetFromCenterOfDiamond(drawPoint, width, height);


                Rectangle dest = new Rectangle(
                  (int)pos.X,
                  (int)pos.Y - yOffset,
                  width,
                  height
              );

                Rectangle source = contr.GetCurrentFrame();
                Texture2D texture = animation.SpriteSheet;

                float frameFade = 1;

                if (animation.FadeEffect)
                    frameFade = 1 - contr.FadeMultiplier;

                float rotation = animation.GetRotation();
                Vector2 origin = animation.GetOrigin();

                SpriteEffects flip = flipHorizontal
                         ? SpriteEffects.FlipHorizontally
                         : SpriteEffects.None;


                spriteBatch.Draw(
                    texture,
                    dest,
                    source,
                    Color.White,
                    rotation,                  // rotation
                    origin,      // origin
                    flip,                // 👈 flip goes here
                    0f                   // layerDepth
                );
                if (animation.FadeEffect)
                {
                    Rectangle source2 = contr.GetNextFrame();
                    spriteBatch.Draw(
                         texture,
                         dest,
                          source2,
                          Color.White,
                          0f,
                          Vector2.Zero,
                          flip,
                         0f
                          );
                }
                if (DebugBugger.ShowAnimationDebug()) DebugBugger.DrawAnimationDebugOutLines(dest, rotation, origin, source, flip);
            }
        }
        public void SetStartingPoint(Vector2 centerPoint)
        {
            MovementController.CurrentPos = centerPoint;
            MovementController.DrawPoint = centerPoint;
        }

        public void DrawCloudMovement(SpriteBatch spriteBatch, Effect fx)
        {
            var animMgr = MovementController.AnimationManager;
            if (animMgr.CurrentControllers == null) return;

            foreach (var contr in animMgr.CurrentControllers)
            {
                var animation = contr.Animation;
                if (animation == null) continue;
                if (contr.IsFinished && !animation.IsLooping && !animation.HoldUntilAllFinished) continue;

                var clouds = animation.FXEntityCloud?.ListOfSpecificEntityClouds;
                if (clouds == null || clouds.Count == 0) continue;

                int width = animation.Width;
                int height = animation.Height;

                Vector2 drawPoint = animation.DrawPointOverride ?? MovementController.DrawPoint;
                Vector2 pos = animation.OverrideDiamondDrawPoint
                    ? drawPoint
                    : TileManager.OffSetFromCenterOfDiamond(drawPoint, width, height);

                var destRect = new Rectangle((int)pos.X, (int)pos.Y - animation.YOffset, width, height);
                var overlaySrc = new Rectangle(0, 0, width, height);

                float rotation = animation.GetRotation();
                Vector2 origin = animation.GetOrigin();
                bool flipHorizontal = MovementController.FlipHorizontally(animation.DefaultDirection);
                if (animation.RotatesTowardDirection) flipHorizontal = false;
                var flip = flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None;


                float remaining = contr.GetRemainingTime();
                float maxDur = animation.FrameDuration;
                float t = 1f - (remaining / maxDur); 
                float wCurr = 1f - t; 
                float wNext = t;

                int iCurr = Math.Clamp(contr.GetCurrentFrameIndex(), 0, animation.FrameCount - 1);
                int iNext = Math.Clamp(contr.GetNextFrameIndex(), 0, animation.FrameCount - 1);

                foreach (var f in clouds)
                {
                    // params that will be the same for both of the fades
                    fx.Parameters["MaskTexture"]?.SetValue(animation.SpriteSheet);
                    fx.Parameters["ScrollSpeed"]?.SetValue(f.ScrollSpeed);
                    fx.Parameters["OverlayColor"]?.SetValue(f.OverlayColor);

                    // each fades specific params
                    Rectangle rCurr = f.GetMask(iCurr);
                    fx.Parameters["MaskUVScale"]?.SetValue(new Vector2(
                        (float)rCurr.Width / animation.SpriteSheet.Width,
                        (float)rCurr.Height / animation.SpriteSheet.Height));
                    fx.Parameters["MaskUVOffset"]?.SetValue(new Vector2(
                        (float)rCurr.X / animation.SpriteSheet.Width,
                        (float)rCurr.Y / animation.SpriteSheet.Height));

                    spriteBatch.Draw(f.OverlayTexture, destRect, overlaySrc,
                                     Color.White * wCurr, rotation, origin, flip, 0f);

                    // second fades params
                    Rectangle rNext = f.GetMask(iNext);
                    fx.Parameters["MaskUVScale"]?.SetValue(new Vector2(
                        (float)rNext.Width / animation.SpriteSheet.Width,
                        (float)rNext.Height / animation.SpriteSheet.Height));
                    fx.Parameters["MaskUVOffset"]?.SetValue(new Vector2(
                        (float)rNext.X / animation.SpriteSheet.Width,
                        (float)rNext.Y / animation.SpriteSheet.Height));

                    spriteBatch.Draw(f.OverlayTexture, destRect, overlaySrc,
                                     Color.White * wNext, rotation, origin, flip, 0f);
                }
            }
        }






    }
}



    


//            {
//"MaskRow": 3,
//              "OverlayTextureName": "TestTexture",
//              "ScrollSpeed": {
//    "X": 0.25,
//                "Y": 0.0
//              },
//              "OverlayColor": {
//    "R": 1.0,
//                "G": 0.4,
//                "B": 0.2,
//                "A": 1.0
//              }
//            }