const commonConfig = require("./webpack.common.js");
const developmentConfig = require("./webpack.development.js");
const merge = require("webpack-merge");

const configs = [commonConfig];

module.exports = env =>
{
    const environment = env.environment;
    console.info(`Environment: ${environment}`);

    if (environment == "development") {
        console.info("Using development config");
        configs.push(developmentConfig(env));
    }

    const config = merge(...configs, {
        mode: environment,
    });

    return config;
};