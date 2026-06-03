mergeInto(LibraryManager.library, {
  onAppStart: function () {
    const result = GameGlobal.dnSDK.onAppStart();

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  SetOpenId: function (openid) {
    var openidStr = UTF8ToString(openid);
    const result = GameGlobal.dnSDK.setOpenId(openidStr);

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onPurchase: function (purchaseValue) {
    const result = GameGlobal.dnSDK.onPurchase(purchaseValue);

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onRegister: function () {
    const result = GameGlobal.dnSDK.onRegister();

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onReActive: function (backFlowDay) {
    const result = GameGlobal.dnSDK.track('RE_ACTIVE', {
      backFlowDay: backFlowDay
    });

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onAddToWishlist: function (type) {
    var typeStr = UTF8ToString(type);
    const result = GameGlobal.dnSDK.track('ADD_TO_WISHLIST', {
      type: typeStr
    });

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onShare: function (target) {
    var targetStr = UTF8ToString(target);
    const result = GameGlobal.dnSDK.track('SHARE', {
      target: targetStr
    });

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onCreateRole: function (roleName) {
    var roleNameStr = UTF8ToString(roleName);
    const result = GameGlobal.dnSDK.onCreateRole(roleNameStr);

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onTutorialFinish: function () {
    const result = GameGlobal.dnSDK.onTutorialFinish();

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onUpdateLevel: function (level, power) {
    const result = GameGlobal.dnSDK.track('UPDATE_LEVEL', {
      level: level,
      power: power,
    });

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onViewContent: function (item) {
    var itemStr = UTF8ToString(item);
    const result = GameGlobal.dnSDK.track('VIEW_CONTENT', {
      item: itemStr,
    });

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  onTrack: function (eventName, propertiesStr) {
    var eventNameStr = UTF8ToString(eventName);
    var propertieStr = UTF8ToString(propertiesStr);
    var properties = JSON.parse(propertieStr) || {};
    const result = GameGlobal.dnSDK.track(eventNameStr, properties);

    var returnStr = JSON.stringify(result);
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  freeMemory: function(ptr) {
    _free(ptr);
  },
});
