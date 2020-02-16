module.exports = env => {
    return {
        devtool: "inline-source-map",
        devServer: {
            port: 9000,
            historyApiFallback: true
        }
    }
};