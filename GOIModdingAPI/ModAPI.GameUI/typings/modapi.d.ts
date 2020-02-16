declare interface Window {
    /**
     * Handles passing data between client code and native code.
     */
    apiEventHandler: ApiEventHandler;
}

declare class ApiEventHandler {
    /**
     * Add an event listener to the specified event name.
     * @param eventName The event to listen for.
     * @param callback The callback function.
     * @param context The optional context object. 'this' will point to this object.
     */
    on(eventName: string, callback: (...args: any) => any, context?: object): void;
    /**
     * Remove an event listener from the specified event name.
     * @param eventName The event to remove the listener from.
     * @param callback The callback that was used when 'on' was called.
     * @param context The context object that was used when 'on' was called.
     */
    off(eventName: string, callback: (...args: any) => any, context?: object): void;
    /**
     * Invoke a native event specified by eventName.
     * @param eventName The event name to invoke.
     * @param args The arguments to pass to the event, if any.
     */
    call(eventName: string, ...args: any[]): any;
}