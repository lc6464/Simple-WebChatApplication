const path = require("path"),
	HtmlWebpackPlugin = require("html-webpack-plugin"),
	{ CleanWebpackPlugin } = require("clean-webpack-plugin"),
	MiniCssExtractPlugin = require("mini-css-extract-plugin"),
	CopyWebpackPlugin = require("copy-webpack-plugin"),
	fs = require("fs");

const files = fs.readdirSync(path.resolve("./src/"), { withFileTypes: true }),
	htmlFiles = files.filter(
		(file) => file.isFile() && file.name.endsWith(".html"),
	),
	entries = htmlFiles.map((file) => file.name.replace(".html", "")),
	entry = {},
	plugins = [];

plugins.push(new CleanWebpackPlugin());

entries.forEach((entryName) => {
	entry[entryName] = `./src/ts/${entryName}.ts`;
	plugins.push(
		new HtmlWebpackPlugin({
			template: `./src/${entryName}.html`,
			filename: `${entryName}.html`,
			chunks: [entryName],
		}),
	);
});

plugins.push(
	new MiniCssExtractPlugin({
		filename: "css/[name].[chunkhash].css",
	}),
);

plugins.push(
	new CopyWebpackPlugin({
		patterns: [
			{
				from: "./src/wwwroot",
				to: "./",
			},
		],
	}),
);

module.exports = {
	entry,
	output: {
		path: path.resolve("./wwwroot"),
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
				use: [
					MiniCssExtractPlugin.loader,
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
	plugins,
};
