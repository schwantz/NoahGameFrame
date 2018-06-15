//
//  UIFleetLiveView.h
//  FleetVideoUIControls
//
//  Created by Bruce Lo on 2018/4/24.
//  Copyright © 2018年 FUHO. All rights reserved.
//

#import <UIKit/UIKit.h>

NS_CLASS_AVAILABLE_IOS(8_0)@interface UIFleetLiveView : UIView
{
    NSString* Host;
    int Port;
    int Channel;
    NSString* Acc;
    NSString* Pwd;
    NSString* timestamp;
    NSString* DeviceId;
    Boolean isPlaying;
}


-(void) setConnectionParm:(NSString*)host port:(int)port channel:(int)ch acc:(NSString*)acc pwd:(NSString*)pwd deviceId:(NSString*)deviceId;
-(void) playVideo;
-(void) stopVideo;

-(void)setHost:(NSString*)host;
-(NSString*)getHost;

-(void)setPort:(int)port;
-(int)getPort;

-(void)setChannel:(int)ch;
-(int)getChannel;

-(void)setAcc:(NSString*)acc;
-(NSString*)getAcc;

-(void)setPwd:(NSString*)pwd;
-(NSString*)getPwd;

-(void)setDeviceId:(NSString*)deviceId;
-(NSString*)getDeviceId;

@end
