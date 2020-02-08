import { createReducer, PayloadAction } from "@reduxjs/toolkit";
import Plugin from "../data/mod-api/plugin";
import { addPlugin, removePlugin, togglePlugin, TogglePluginPayload, setPlugins } from "../actions/plugins-actions";

type PluginsState = {
    plugins: Plugin[]
}

const getDefaultState = (): PluginsState => ({
    plugins: []
});

export default createReducer<PluginsState>(getDefaultState(), {
    [addPlugin.type]: doAddPlugin,
    [removePlugin.type]: doRemovePlugin,
    [togglePlugin.type]: doTogglePlugin,
    [setPlugins.type]: doSetPlugins
});

function findIndex(plugins: Plugin[], assemblyName: string): number {
    return plugins.findIndex(plugin => plugin.assemblyName == assemblyName);
}

function doAddPlugin(state: PluginsState, action: PayloadAction<Plugin>) {
    state.plugins.push(action.payload);
}

function doRemovePlugin(state: PluginsState, action: PayloadAction<string>) {
    const pluginIndex = findIndex(state.plugins, action.payload);

    if (pluginIndex != -1) {
        state.plugins.splice(pluginIndex, 1);
    }
}

function doTogglePlugin(state: PluginsState, action: PayloadAction<TogglePluginPayload>) {
    const pluginIndex = findIndex(state.plugins, action.payload.assemblyName);

    if (pluginIndex != -1) {
        state.plugins[pluginIndex].enabled = action.payload.enabled;
    }
}

function doSetPlugins(state: PluginsState, action: PayloadAction<Plugin[]>) {
    state.plugins = [];
    state.plugins.push(...action.payload);
}