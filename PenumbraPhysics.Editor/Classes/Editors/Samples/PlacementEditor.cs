﻿using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using PenumbraPhysics.Editor.Classes.Basic;
using System.Collections.Generic;

namespace PenumbraPhysics.Editor.Classes.Editors.Samples
{
    public class PlacementEditor : GFXPhysicsService
    {
        public List<Light> LightObjectList;
        public List<Body> ShadowObjectList;
        public List<Body> AllObjectList;

        public Body CurrentSelectedObject;

        private Texture2D CenterTexture, MoveCamTexture;

        public void CreateLightObject()
        {
            if (LightObjectList != null && AllObjectList != null)
            {
                Vector2 position = new Vector2(ViewportWidth / 2, ViewportHeight / 2);

                //Create a light
                Light light = new PointLight
                {
                    Position = position,
                    Color = Color.White,
                    Scale = new Vector2(400),
                    ShadowType = ShadowType.Solid
                };
                LightObjectList.Add(light);
                Penumbra.Lights.Add(light);
                
                //Create a pivot for the light
                Body bPivot = BodyFactory.CreateCircle(_World, ConvertUnits.ToSimUnits(5f), 1f);
                bPivot.Position = ConvertUnits.ToSimUnits(position);
                bPivot.BodyType = BodyType.Dynamic;
                bPivot.IsSensor = true;
                bPivot.FixedRotation = true;
                bPivot.LinearDamping = 999f;
                bPivot.Friction = 999f;
                bPivot.CollidesWith = Category.None;
                bPivot.CollisionCategories = Category.None;
                bPivot.UserData = new PivotBodyFlags()
                {
                    ConnectedObject = new ConnectedObject(light, position)
                };
                CurrentSelectedObject = bPivot;
                AllObjectList.Add(bPivot);
            }
        }
        public void CreateShadowObject()
        {
            Vector2 position = new Vector2(ViewportWidth / 2, ViewportHeight / 2);

            Body ShadowCaster = BodyFactory.CreateRectangle(_World,
                ConvertUnits.ToSimUnits(20), ConvertUnits.ToSimUnits(35), 1f);
            ShadowCaster.Position = ConvertUnits.ToSimUnits(position);
            ShadowCaster.AngularDamping = 999f;
            ShadowCaster.LinearDamping = 999f;
            ShadowCaster.Friction = 3f;
            ShadowCaster.BodyType = BodyType.Dynamic;
            ShadowCaster.UserData = new BodyFlags()
            {
                HullList = new List<Hull>()
            };

            CreateShadowHulls(ShadowCaster);

            CurrentSelectedObject = ShadowCaster;
            ShadowObjectList.Add(ShadowCaster);
            AllObjectList.Add(ShadowCaster);
        }

        public void RemoveAllObjects()
        {
            AllObjectList.ForEach(o =>
            {
                if (o.UserData != null && o.UserData is PivotBodyFlags)
                {
                    if (((PivotBodyFlags)o.UserData).ConnectedObject != null &&
                        ((PivotBodyFlags)o.UserData).ConnectedObject.Object != null &&
                        ((PivotBodyFlags)o.UserData).ConnectedObject.Object is Light)
                    {
                        Penumbra.Lights.Remove((Light)((PivotBodyFlags)o.UserData).ConnectedObject.Object);
                        _World.RemoveBody(o);
                    }
                }
                else if (o.UserData != null && o.UserData is BodyFlags)
                {
                    if (((BodyFlags)o.UserData).HullList != null)
                    {
                        ((BodyFlags)o.UserData).HullList.ForEach(a => a.Enabled = false && Penumbra.Hulls.Remove(a));
                        ((BodyFlags)o.UserData).HullList.Clear();
                        _World.RemoveBody(o);
                    }
                }
            });
            Penumbra.Lights.Clear();
            Penumbra.Hulls.Clear();
            ShadowObjectList.Clear();
            LightObjectList.Clear();
            AllObjectList.Clear();
        }

        public PlacementEditor(IGraphicsDeviceService graphics)
        {
            // Initialize GFX-System
            InitializeGFX(graphics);

            // Initialize Physics-System
            InitializePhysics(graphics.GraphicsDevice, Content);
        }

        public void Initialize()
        {
            Penumbra.AmbientColor = new Color(new Vector3(0.7f));

            LightObjectList = new List<Light>();
            ShadowObjectList = new List<Body>();
            AllObjectList = new List<Body>();

            CenterTexture = Content.Load<Texture2D>(@"Samples\PenumbraPhysicsEditorLogoBig");
            MoveCamTexture = Content.Load<Texture2D>(@"Samples\MoveCam");
        }

        public void Update(GameTime gameTime, Vector2 mousePosition, bool leftMouseButtonPressed)
        {
            if (FixedMouseJoint != null)
            {
                if (FixedMouseJoint.BodyA != null) CurrentSelectedObject = FixedMouseJoint.BodyA;
            }

            UpdateShadowHulls(ShadowObjectList);
            UpdatePhysicsManipulation(leftMouseButtonPressed, mousePosition);
            UpdatePhysics(gameTime);
            UpdateDisplay(gameTime, mousePosition);
        }

        public void Draw(GameTime gameTime)
        {
            UpdateFrameCounter();

            // Everything between penumbra.BeginDraw and penumbra.Draw will be
            // lit by the lighting system.

            Penumbra.BeginDraw();

            graphics.Clear(BackgroundColor);

            DrawBeginCamera2D();

            //Draw sprites here, so they will affected by the camera movement

            spriteBatch.Draw(CenterTexture, new Vector2(ViewportWidth / 2, ViewportHeight / 2), Color.White);
            spriteBatch.Draw(MoveCamTexture, new Vector2(-150 + ViewportWidth / 2, ViewportHeight / 2), null, Color.White,
                0f, new Vector2(MoveCamTexture.Width / 2, MoveCamTexture.Height / 2), 1f, SpriteEffects.None, 0f);

            DrawEndCamera2D();

            DrawPhysicsDebugView();
            Penumbra.Draw();

            DrawDisplay();
        }
    }
}
