﻿using ClosedXML.Excel;
using KoOrderRegister.Modules.Windows.Notification.Utils;
using KoOrderRegister.Utility;
using KORCore.Utility;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.iOSOption;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;



namespace KoOrderRegister.Services
{
    public class LocalNotificationService : ILocalNotificationService
    {
        public event Action<NotificationActionArgs> NotificationReceived;
        public event Action<NotificationChangedArgs> NotificationChanged;
        public event Action<NotificationClearedArgs> NotificationCleared;

        public static event Action NotificationChangedStaic;

        private static ConcurrentDictionary<int, NotificationChangedArgs> Notifications = new ConcurrentDictionary<int, NotificationChangedArgs>();
        public LocalNotificationService()
        {
            LocalNotificationCenter.Current.NotificationActionTapped += Current_NotificationActionTapped;
            NotificationCleared += LocalNotification_NotificationCleared;
        }


        private async void LocalNotification_NotificationCleared(NotificationClearedArgs obj)
        {
            LocalNotificationService.Notifications.TryRemove(obj.Id, out _);
        }
        private void LocalNotification_NotificationChanged(NotificationChangedArgs obj)
        {
            LocalNotificationService.Notifications.AddOrUpdate(
            obj.NotificationRequest.NotificationId,
            obj,
            (key, existingVal) => obj);
            NotificationChanged?.Invoke(obj);
            NotificationChangedStaic?.Invoke();
        }

        private void Current_NotificationActionTapped(Plugin.LocalNotification.EventArgs.NotificationActionEventArgs e)
        {
            ClickNotification(new NotificationActionArgs
            {
                Id = e.Request.NotificationId,
                Title = e.Request.Title,
            });
        }
        public void ClickNotification(NotificationActionArgs notificationArgs)
        {
            NotificationReceived?.Invoke(notificationArgs);
        }

        private void ChangeNotification(NotificationRequest notificationRequest, AndroidOptions androidOptions, iOSOptions iosOption, WindowsOptions windowsOptions, bool cleared = false)
        {
            NotificationChangedArgs noty = new NotificationChangedArgs
            {
                AndroidOption = androidOptions,
                NotificationRequest = notificationRequest,
                IosOption = iosOption,
                WindowsOption = windowsOptions,
                IsProgressBar =
#if WINDOWS
                windowsOptions == null ? false :windowsOptions.ProgressBar == null ? false : true
#elif ANDROID
                androidOptions == null ? false : androidOptions.ProgressBar == null ? false : true
#elif IOS
                IosOption == null ? false : IosOption.PresentAsBanner == null ? false : true
#elif MACOS
                IosOption == null ? false : IosOption.PresentAsBanner == null ? false : true
#else
                 false
#endif
            };
            NotificationChanged?.Invoke(noty);
            LocalNotificationCenter.Current.Show(notificationRequest);
            ThreadManager.Run(() => LocalNotification_NotificationChanged(noty), ThreadManager.Priority.Low);
        }
        private void ClearNotification(int id)
        {
            NotificationCleared?.Invoke(new NotificationClearedArgs { Id = id });
            NotificationChangedStaic.Invoke();
            LocalNotificationCenter.Current.Clear(id);
        }


        public int SendNotification(string title, string message, NotificationCategoryType categoryType = NotificationCategoryType.None, AndroidOptions androidOptions = null, iOSOptions iosOption = null, WindowsOptions windowsOptions = null)
        {
            int id = new Random().Next(0, int.MaxValue);
            var request = new NotificationRequest
            {
                NotificationId = id,
                Title = title,
                Description = message,
                CategoryType = categoryType,
                Silent = false,
                Android =
                {
                    ChannelId = "kor_general",
                    IconSmallName =
                    {
                          ResourceName = "appicon",
                    },
                }
            };

            if (androidOptions != null)
            {
                request.Android = androidOptions;
            }
            if (iosOption != null)
            {
                request.iOS = iosOption;
            }

            ChangeNotification(request, androidOptions, iosOption, windowsOptions);
            return id;
        }

        public void UpdateNotification(int id, string title, string message, NotificationCategoryType categoryType = NotificationCategoryType.None, AndroidOptions androidOptions = null, iOSOptions iosOption = null, WindowsOptions windowsOptions = null)
        {
            var request = new NotificationRequest
            {
                NotificationId = id,
                Title = title,
                Description = message,
                CategoryType = categoryType,
                Silent = true,
                Android =
                {
                    ChannelId = "kor_general",
                    IconSmallName =
                    {
                          ResourceName = "appicon",
                    },
                    PendingIntentFlags = AndroidPendingIntentFlags.UpdateCurrent,
                }               
            };

            if (androidOptions != null)
            {
                request.Android = androidOptions;
            }
            if (iosOption != null)
            {
                request.iOS = iosOption;
            }

            ChangeNotification(request, androidOptions, iosOption, windowsOptions);
        }

        public void DeleteNotification(int id)
        {
            ClearNotification(id);
        }

        public static int GetNotificationCount()
        {
            if(LocalNotificationService.Notifications != null)
            {
                return LocalNotificationService.Notifications.Count;
            }
            return 0;
        }

    }
}
