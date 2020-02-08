import { createAction } from "@reduxjs/toolkit";

export type TogglePluginPayload = {
    assemblyName: string;
    enabled: boolean;
};

export const addPlugin = createAction<Plugin>("ADD_PLUGIN");
export const removePlugin = createAction<string>("REMOVE_PLUGIN");
export const togglePlugin = createAction<TogglePluginPayload>("TOGGLE_PLUGIN");
export const setPlugins = createAction<Plugin[]>("SET_PLUGINS");