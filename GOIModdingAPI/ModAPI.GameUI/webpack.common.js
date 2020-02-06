const HtmlWebpackPlugin = require("html-webpack-plugin");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const path = require("path");
const xmlParser = require("fast-xml-parser");
const fs = require("fs");

// Load DevVars from parent directory
if (!fs.existsSync("../DevVars.targets")) {
    throw "DevVars.targets file not found. Make sure your project is properly initialized.";
}

const devVarsContents = fs.readFileSync("../DevVars.targets").toString();

if (xmlParser.validate(devVarsContents) !== true) {
    throw "DevVars.targets file does not contain valid xml.";
}

const devVars = xmlParser.parse(devVarsContents);
const gameDirectory = devVars.Project.PropertyGroup.GameDirectory;

console.info(`Game directory: ${gameDirectory}`);

module.exports = {
    entry: "./src/index.tsx",
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: "ts-loader",
                exclude: /node_modules/
            },
            {
                test: /\.s[ac]ss$/i,
                use: [
                    "style-loader",
                    "css-loader",
                    "sass-loader"
                ]
            },
            {
                test: /\.ttf$/,
                use: [
                    {
                        loader: "file-loader",
                        options: {
                            name: "./fonts/[name].[ext]"
                        }
                    }
                ]
            }
        ]
    },
    resolve: {
        extensions: [".tsx", ".ts", ".js"]
    },
    plugins: [
        new CleanWebpackPlugin(),
        new HtmlWebpackPlugin({
            title: "Game UI - Getting Over It",
            template: "./public/index.ejs"
        })
    ],
    output: {
        filename: "[name].bundle.js",
        path: path.resolve(__dirname, gameDirectory, "gameui")
    }
};