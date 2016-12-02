NSString* CreateNSString (const char* string)
{
	if (string)
		return [NSString stringWithUTF8String: string];
	else
		return [NSString stringWithUTF8String: ""];
}

NSString* serializeURL(NSDictionary* params, NSString* appBaseURL) {
	NSURL *parsedURL = [NSURL URLWithString:appBaseURL];
	NSString *queryPrefix = parsedURL.query ? @"&" : @"?";
	
	NSMutableArray *pairs = [NSMutableArray array];
	for (NSString *key in [params keyEnumerator]) {
		[pairs addObject:[NSString stringWithFormat:@"%@=%@", key, params[key]]];
	}
	NSString *query = [pairs componentsJoinedByString:@"&"];
	
	return [NSString stringWithFormat:@"%@%@%@", @"okauth://authorize", queryPrefix, query];
}

bool isNativeAppInstalled(NSString* appId, NSString* scope) {
	NSString *appBaseURL = [NSString stringWithFormat:@"ok%@://authorize", appId];
	NSMutableDictionary *params = [NSMutableDictionary new];
	params[@"client_id"] = appId;
	params[@"redirect_uri"] = appBaseURL;
	params[@"response_type"] = @"token";
	params[@"scope"] = scope;
	
	NSURL *authorizeUrl = [NSURL URLWithString:serializeURL(params, appBaseURL)];
	NSURL *callbackUrl = [NSURL URLWithString:appBaseURL];
	UIApplication *app = [UIApplication sharedApplication];
	return [app canOpenURL:authorizeUrl] && [app canOpenURL:callbackUrl];
}

void authorizeInApp(NSString* appId, NSString* scope) {
	NSString *appBaseURL = [NSString stringWithFormat:@"ok%@://authorize", appId];
	NSMutableDictionary *params = [NSMutableDictionary new];
	params[@"client_id"] = appId;
	params[@"redirect_uri"] = appBaseURL;
	params[@"response_type"] = @"token";
	params[@"scope"] = scope;

	UIApplication *app = [UIApplication sharedApplication];

	NSURL *authorizeUrl = [NSURL URLWithString:serializeURL(params, appBaseURL)];
	if (isNativeAppInstalled(appId, scope)) {
		[app openURL:authorizeUrl];
	} else {
		NSLog(@"SSO Authorization failed");
		UnitySendMessage("Odnoklassniki", "SSOAuthFailed", [@"No OK app found" UTF8String]);
	}
}



extern "C" {
	void _authorizeInApp(const char* appId, const char* scope) {
		NSString* nsAppId = CreateNSString(appId);
		NSString* nsScope = CreateNSString(scope);
		authorizeInApp(nsAppId, nsScope);
	}
	
	bool _isNativeAppInstalled(const char* appId, const char* scope) {
		NSString* nsAppId = CreateNSString(appId);
		NSString* nsScope = CreateNSString(scope);
		return isNativeAppInstalled(nsAppId, nsScope);
	}
}