using System.Collections;
using System.Collections.Generic;
using GravityEngine;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Utils;

using GravityEngine.Platform;
using UnityEngine;

/// <summary>
/// 微信小游戏事件自动采集类，要求引用Unity WebGL 微信小游戏适配方案中的Unity插件，参考：https://github.com/wechat-miniprogram/minigame-unity-webgl-transform
/// </summary>
public class GEMiniGameAutoTrack : MonoBehaviour
{
    private AUTO_TRACK_EVENTS _mAutoTrackEvents = AUTO_TRACK_EVENTS.NONE;

    private readonly Dictionary<string, Dictionary<string, object>> _mAutoTrackProperties =
        new Dictionary<string, Dictionary<string, object>>();

    private Dictionary<string, object> GetAutoTrackProperties(string eventName)
    {
        Dictionary<string, object> properties = new Dictionary<string, object>();
        if (_mAutoTrackProperties.ContainsKey(eventName))
        {
            GravitySDKUtil.AddDictionary(properties, _mAutoTrackProperties[eventName]);
        }

        return properties;
    }

    public void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties = null)
    {
        SetAutoTrackProperties(events, properties);
        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_SHOW) || true)
        {
            if (GEPlatformManager.isMiniGame())
            {
                GEPlatformManager.addShowAutoTrack((trackEventStr) =>
                {
                    return GetAutoTrackProperties(trackEventStr);
                });
            }
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_HIDE) || true)
        {
            if (GEPlatformManager.isMiniGame())
            {
                GEPlatformManager.addHideAutoTrack((trackEventStr) =>
                {
                    return GetAutoTrackProperties(trackEventStr);
                });
            }
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES) || true)
        {
            if (GEPlatformManager.isMiniGame())
            {
                GEPlatformManager.addFavoriteAutoTrack((trackEventStr) =>
                {
                    return GetAutoTrackProperties(trackEventStr);
                });
            }
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_SHARE) || true)
        {
            if (GEPlatformManager.isMiniGame())
            {
                GEPlatformManager.addShareAutoTrack((trackEventStr) =>
                {
                    return GetAutoTrackProperties(trackEventStr);
                });
            }
        }
    }

    public void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
    {
        _mAutoTrackEvents = events;
        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_SHOW) || true)
        {
            if (_mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.MP_SHOW.ToString()))
            {
                GravitySDKUtil.AddDictionary(_mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_SHOW.ToString()],
                    properties);
            }

            _mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_SHOW.ToString()] = properties;
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_HIDE) || true)
        {
            if (_mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.MP_HIDE.ToString()))
            {
                GravitySDKUtil.AddDictionary(_mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_HIDE.ToString()],
                    properties);
            }

            _mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_HIDE.ToString()] = properties;
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_SHARE) || true)
        {
            if (_mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.MP_SHARE.ToString()))
            {
                GravitySDKUtil.AddDictionary(_mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_SHARE.ToString()],
                    properties);
            }

            _mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_SHARE.ToString()] = properties;
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES) || true)
        {
            if (_mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES.ToString()))
            {
                GravitySDKUtil.AddDictionary(_mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES.ToString()],
                    properties);
            }

            _mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES.ToString()] = properties;
        }
    }
}