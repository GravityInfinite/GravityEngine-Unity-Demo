//
//  GravityEngineUnityChannel.m
//  UnityFramework
//
//  Created by leo on 2026/1/9.
//

#import <Foundation/Foundation.h>

const char* ge_channel_strdup(const char* string) {
    if (string == NULL)
        return NULL;
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

typedef const char * (*GEiOSCallUnityHandler) (const char *type, const char *jsonData);
static GEiOSCallUnityHandler geOniOSCallUnityHandler;
extern "C" void ge_initUnityNativeChannel(GEiOSCallUnityHandler geHandlerPointer)
{
    geOniOSCallUnityHandler = geHandlerPointer;
}

extern "C" const char * ge_onUnityCallNative(const char *functionName,const char *dataString) {
    Class cls = NSClassFromString(@"GravityEngineUnityAPI");
    SEL sel = NSSelectorFromString(@"onUnityCallNative:data:");
    if(cls && [cls respondsToSelector:sel]){
        NSMethodSignature *sig = [cls methodSignatureForSelector:sel];
        NSInvocation *invocation = [NSInvocation invocationWithMethodSignature:sig];
        invocation.selector = sel;
        invocation.target = cls;

        NSString *name = [NSString stringWithUTF8String:functionName];
        NSString * data = [NSString stringWithUTF8String:dataString];

        [invocation setArgument:&name atIndex:2];
        [invocation setArgument:&data atIndex:3];
        [invocation invoke];
        
        __unsafe_unretained NSString * resultTemp = nil;
        [invocation getReturnValue:&resultTemp];
        NSString * result = resultTemp;
        
        return ge_channel_strdup([result UTF8String]);
    }
    return ge_channel_strdup([@"" UTF8String]);
}

const char * ge_onNativeCallUnity(const char *functionName,const char *dataString) {
    if(geOniOSCallUnityHandler){
        return geOniOSCallUnityHandler(functionName,dataString);
    }
    return ge_channel_strdup([@"" UTF8String]);
}

@interface GravityEngineUnityChannel : NSObject

@end

@implementation GravityEngineUnityChannel

+(NSString *)onNeedCallUnity:(NSString *)functionName msg:(NSString *)msg{
    const char * functionNameChar  = ge_channel_strdup([functionName UTF8String]);
    const char * msgChar  = ge_channel_strdup([msg UTF8String]);
    const char * resultChar = ge_onNativeCallUnity(functionNameChar,msgChar);
    return [NSString stringWithUTF8String:resultChar];
}
@end
