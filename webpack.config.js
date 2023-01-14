const path = require("path"),
	HtmlWebpackPlugin = require("html-webpack-plugin"),
	{ CleanWebpackPlugin } = require("clean-webpack-plugin"),
	MiniCssExtractPlugin = require("mini-css-extract-plugin"),
	CopyWebpackPlugin = require("copy-webpack-plugin");

module.exports = {
	entry: "./src/index.ts",
	output: {
		path: path.resolve(__dirname, "wwwroot"),
		filename: "[name].[chunkhash].js",
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