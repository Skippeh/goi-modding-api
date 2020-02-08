type Plugin = {
    /** The name of this plugin. */
    name: string;
    /** The assemly name of this plugin (unique). */
    assemblyName: string;
    /** A short description of this plugin. */
    shortDescription: string;
    /** The author of this plugin. */
    author: string;
    /** The version of this plugin. */
    version: string;
    /** The mod api version this mod works on. */
    modApiVersion: string;
    /** An array of plugins this plugin depends on. */
    dependencies: Dependency;
    /** True if the mod is enabled. */
    enabled: boolean;
};

export type Dependency = {
    name: string;
    version: string;
};

export default Plugin;