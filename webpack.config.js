const path = require("path"),
	HtmlWebpackPlugin = require("html-webpack-plugin"),
	{ CleanWebpackPlugin } = require("clean-webpack-plugin"),
	MiniCssExtractPlugin = require("mini-css-extract-plugin"),
	CopyWebpackPlugin = require("copy-webpack-plugin");

module.exports = {
	mode: "development",
	entry: {
		index: "./src/ts/index.ts",
		login: "./src/ts/login.ts",
		chat: "./src/ts/chat.ts",
	},
	output: {
		path: path.resolve(__dirname, "wwwroot"),
		filename: "js/[name].[chunkhash].js",
		publicPath: "/",
	},
	resolve: {
		extensions: [".js", ".ts"],
	},
	module: {
		rules: [
			{
				test: /\.ts$/,
				use: "ts-loader",
			},
			{
				test: /\.css$/,
				use: [MiniCssExtractPlugin.loader, "css-loader"],
			},
		],
	},
	plugins: [
		new CleanWebpackPlugin(),
		new HtmlWebpackPlugin({
			template: "./src/index.html",
			filename: "index.html"
		}),
		new HtmlWebpackPlugin({
			template: "./src/login.html",
			filename: "login.html"
		}),
		new HtmlWebpackPlugin({
			template: "./src/chat.html",
			filename: "chat.html"
		}),
		new MiniCssExtractPlugin({
			filename: "css/[name].[chunkhash].css",
		}),
		new CopyWebpackPlugin({
			patterns: [
				{
					from: "./src/wwwroot",
					to: "./",
				},
			],
		}),
	],
};