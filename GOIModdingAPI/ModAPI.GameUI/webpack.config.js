const commonConfig = require("./webpack.common.js");
const merge = require("webpack-merge");

const configs = [commonConfig];

module.exports = env =>
{
    const environment = env.environment;
    console.info(`Environment: ${environment}`);

    if (environment == "development") {
        configs.push(require("./webpack.development.js"));
    }

    return merge(...configs, {
        mode: environment,
    });
};