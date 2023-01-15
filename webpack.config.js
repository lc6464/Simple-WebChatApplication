const path = require("path"),
	HtmlWebpackPlugin = require("html-webpack-plugin"),
	{ CleanWebpackPlugin } = require("clean-webpack-plugin"),
	MiniCssExtractPlugin = require("mini-css-extract-plugin"),
	CopyWebpackPlugin = require("copy-webpack-plugin");

module.exports = {
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
				use: [MiniCssExtractPlugin.loader,
					{
						loader: "css-loader",
						options: {
							url: false,
						},
					},
				],
			},
		],
	},
	plugins: [
		new CleanWebpackPlugin(),
		new HtmlWebpackPlugin({
			template: "./src/index.html",
			filename: "index.html",
			chunks: ["index"],
		}),
		new HtmlWebpackPlugin({
			template: "./src/login.html",
			filename: "login.html",
			chunks: ["login"],
		}),
		new HtmlWebpackPlugin({
			template: "./src/chat.html",
			filename: "chat.html",
			chunks: ["chat"],
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