﻿using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using System;
using UniRx;
using UnityEngine;

namespace Stiletto
{
    public static class StilettoGui
    {
        private static MakerSlider slider_AngleAnkle;
        private static MakerSlider slider_AngleLeg;
        private static MakerSlider slider_Height;

        public static void Init(Stiletto plugin)
        {
            if(StudioAPI.InsideStudio)
                RegisterStudioControls();
            else
                MakerAPI.RegisterCustomSubCategories += (sender, e) => RegisterMakerControls(plugin, e);
        }

        private static void RegisterMakerControls(Stiletto plugin, RegisterSubCategoriesEvent e)
        {
            // Doesn't apply to male characters
            if(MakerAPI.GetMakerSex() == 0) return;

            var shoesCategory = MakerConstants.Clothes.OuterShoes;
            var category = new MakerCategory(shoesCategory.CategoryName, "stiletto", shoesCategory.Position + 1, "Stiletto");
            e.AddSubCategory(category);

            slider_AngleAnkle = e.AddControl(new MakerSlider(category, "AngleAnkle", 0f, 60f, 0f, plugin) { StringToValue = CreateStringToValueFunc(10f), ValueToString = CreateValueToStringFunc(10f),  });
            slider_AngleLeg = e.AddControl(new MakerSlider(category, "AngleLeg", 0f, 60f, 0f, plugin) { StringToValue = CreateStringToValueFunc(10f), ValueToString = CreateValueToStringFunc(10f) });
            slider_Height = e.AddControl(new MakerSlider(category, "Height", 0f, 0.5f, 0f, plugin) { StringToValue = CreateStringToValueFunc(1000f), ValueToString = CreateValueToStringFunc(1000f) });

            slider_AngleAnkle.BindToFunctionController<HeelInfoController, float>(ctrl => ctrl.AngleA.eulerAngles.x, (ctrl, f) => ctrl.UpdateAnkleAngle(f));
            slider_AngleLeg.BindToFunctionController<HeelInfoController, float>(ctrl => ctrl.AngleLeg.eulerAngles.x, (ctrl, f) => ctrl.UpdateLegAngle(f));
            slider_Height.BindToFunctionController<HeelInfoController, float>(ctrl => ctrl.Height.y, (ctrl, f) => ctrl.UpdateHeight(f));
        }

        public static void UpdateMakerValues(float angleAnkle, float angleLeg, float height)
        {
            if(slider_AngleAnkle != null)
            {
                slider_AngleAnkle.Value = angleAnkle;
                slider_AngleLeg.Value = angleLeg;
                slider_Height.Value = height; 
            }
        }

        public static Func<string, float> CreateStringToValueFunc(float multi)
        {
            return new Func<string, float>(txt => float.Parse(txt) / multi);
        }

        public static Func<float, string> CreateValueToStringFunc(float multi)
        {
            return new Func<float, string>(f => Mathf.RoundToInt(f * multi).ToString());
        }

        private static void RegisterStudioControls()
        {
            var slider_AngleAnkle = CreateSlider("AngleAnkle", x => x.AngleA.eulerAngles.x, (x, y) => x.UpdateAnkleAngle(y), 0f, 60f);
            var slider_AngleLeg = CreateSlider("AngleLeg", x => x.AngleLeg.eulerAngles.x, (x, y) => x.UpdateLegAngle(y), 0f, 60f);
            var slider_Height = CreateSlider("Height", x => x.Height.y, (x, y) => x.UpdateHeight(y), 0f, 0.5f);

            StudioAPI.GetOrCreateCurrentStateCategory("Stiletto").AddControls(slider_AngleAnkle, slider_AngleLeg, slider_Height);

            CurrentStateCategorySlider CreateSlider(string name, Func<HeelInfoController, float> get, Action<HeelInfoController, float> set, float minValue, float maxValue)
            {
                var slider = new CurrentStateCategorySlider(name, (chara) => get(chara.charInfo.GetComponent<HeelInfoController>()), minValue, maxValue);
                slider.Value.Subscribe(Observer.Create((float x) =>
                {
                    foreach(var heelInfo in StudioAPI.GetSelectedControllers<HeelInfoController>())
                        set(heelInfo, x);
                }));

                return slider;
            }
        }
    }
}
