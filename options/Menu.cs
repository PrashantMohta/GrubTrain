using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using Patch = Modding.Patches;
using UnityEngine;
using UnityEngine.UI;
using Satchel;

namespace GrubTrain
{
     public class Menu
    {
        public static MenuScreen Screen;
        public static MenuScreen modListMenu; 

        public static void saveModsMenuScreen(MenuScreen screen){
            modListMenu = screen;
        }

        public static void BackSetting(){
            GrubTrain.Instance.destroyTrain();
            GoToModListMenu();
        }

        public static void GoToModListMenu(object _) {
            GoToModListMenu();
        }
        public static void GoToModListMenu() {
            UIManager.instance.UIGoToDynamicMenu(modListMenu); 
        }
        private static void OpenLink(object _) => OpenLink();

        private static void OpenLink(){ 
            Application.OpenURL("https://discord.gg/rqsRHRt25h");
        }

        private static void addMenuOptions(ContentArea area){

            area.AddTextPanel("HelpText",
                    new RelVector2(new Vector2(950f, 125f)),
                    new TextPanelConfig{
                        Text = "GrubTrain",
                        Size = 30,
                        Font = TextPanelConfig.TextFont.TrajanRegular,
                        Anchor = TextAnchor.MiddleCenter
                    });  

            area.AddHorizontalOption(
                    "Grub Strats",
                    new HorizontalOptionConfig
                    {
                        Options = new string[] { "Enabled" , "Disabled"},
                        ApplySetting = (_, i) => GrubTrain.settings.grubStrats = (i == 0),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(GrubTrain.settings.grubStrats ? 0 : 1),
                        CancelAction = _ => { BackSetting(); },
                        Description = new DescriptionInfo
                        {
                            Text = "Epic Grub tricks",
                            Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
                        },
                        Label = "Grub Strats",
                        Style = HorizontalOptionStyle.VanillaStyle
                    },
                    out var grubStratsSelector
                );  

            area.AddHorizontalOption(
                    "Cursed Strats",
                    new HorizontalOptionConfig
                    {
                        Options = new string[] { "Enabled" , "Disabled"},
                        ApplySetting = (_, i) => GrubTrain.settings.cursedStrats = (i == 0),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(GrubTrain.settings.cursedStrats ? 0 : 1),
                        CancelAction = _ => { BackSetting(); },
                        Description = new DescriptionInfo
                        {
                            Text = "Cursed Grub tricks",
                            Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
                        },
                        Label = "Cursed Strats",
                        Style = HorizontalOptionStyle.VanillaStyle
                    },
                    out var cursedStratsSelector
                );  

            area.AddHorizontalOption(
                    "Grub Gatherer Mode",
                    new HorizontalOptionConfig
                    {
                        Options = new string[] { "Enabled" , "Disabled"},
                        ApplySetting = (_, i) => GrubTrain.settings.grubGathererMode = (i == 0),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(GrubTrain.settings.grubGathererMode ? 0 : 1),
                        CancelAction = _ => { BackSetting(); },
                        Description = new DescriptionInfo
                        {
                            Text = "Add freed grubs to the train",
                            Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
                        },
                        Label = "Grub Gatherer Mode",
                        Style = HorizontalOptionStyle.VanillaStyle
                    },
                    out var grubGathererSelector
                ); 


            area.AddHorizontalOption(
                    "Grub Sounds",
                    new HorizontalOptionConfig
                    {
                        Options = new string[] { "Enabled" , "Disabled"},
                        ApplySetting = (_, i) => GrubTrain.settings.enableSounds = (i == 0),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(GrubTrain.settings.enableSounds ? 0 : 1),
                        CancelAction = _ => { BackSetting(); },
                        Description = new DescriptionInfo
                        {
                            Text = "Enable Grub sounds",
                            Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
                        },
                        Label = "Grub Sounds",
                        Style = HorizontalOptionStyle.VanillaStyle
                    },
                    out var grubSoundsSelector
                ); 

            
            area.AddMenuButton(
                        "DiscordButton",
                        new MenuButtonConfig
                        {
                            Label = "Need Help or Have Suggestions?",
                            CancelAction = GoToModListMenu,
                            SubmitAction = OpenLink,
                            Proceed = true,
                            Style = MenuButtonStyle.VanillaStyle,
                            Description = new DescriptionInfo
                            {
                                Text = "Join the Hollow Knight Modding Discord."
                            }
                        },
                        out MenuButton DiscordButton
                    );   

        }
        public static MenuScreen CreatemenuScreen()
        { 
            var builder =  new MenuBuilder(UIManager.instance.UICanvas.gameObject, "GrubTrain")
                .CreateTitle("GrubTrain", MenuTitleStyle.vanillaStyle)
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 903f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 250f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -502f)
                    )
                ))
                .SetDefaultNavGraph(new ChainedNavGraph())
                .AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -64f)
                    )),
                    c => c.AddMenuButton(
                        "BackButton",
                            new MenuButtonConfig
                            {
                                Label = "Back",
                                CancelAction = _ => { BackSetting(); },
                                SubmitAction = _ => { BackSetting(); } ,
                                Style = MenuButtonStyle.VanillaStyle,
                                Proceed = true
                            }
                        )
                );
                builder.AddContent(new NullContentLayout(), c => c.AddScrollPaneContent(
                new ScrollbarConfig
                {
                    CancelAction = _ => { BackSetting(); },
                    
                    Navigation = new Navigation
                    {
                        mode = Navigation.Mode.Explicit,
                    },
                    Position = new AnchoredPosition
                    {
                        ChildAnchor = new Vector2(0f, 1f),
                        ParentAnchor = new Vector2(1f, 1f),
                        Offset = new Vector2(-310f, 0f)
                    }
                },
                    new RelLength(1600f), 
                    RegularGridLayout.CreateVerticalLayout(125f),
                    contentArea => addMenuOptions(contentArea)
                ));
                Screen = builder.Build();

            return Screen;
        }
    }
}