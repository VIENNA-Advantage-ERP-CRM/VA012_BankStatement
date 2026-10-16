const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const fs = require('fs');

const deleteFilesByPattern = (directory, pattern) => {
    if (!fs.existsSync(directory)) {
        return;
    }
    const files = fs.readdirSync(directory);
    const regex = new RegExp(pattern);
    files.forEach(file => {
        if (regex.test(file)) {
            fs.unlinkSync(path.join(directory, file));
        }
    });
};

// the css is emitted outside the output folder, so clean the previous one manually
deleteFilesByPattern(path.resolve(__dirname, 'Areas/VA012/Contents'), /^VA012\.all\.min\.css$/);

const versions = {
    'VA012.all': '3.0.1.0',
    'VA012': '3.0.1.0' // CSS Version
};

module.exports = {
    //mode: 'development', // for debugging; 'npm run buildPro' builds with --mode production
    mode: 'production',
    entry: {
        'VA012.all': './Areas/VA012/Scripts/src/VA012js.js',
        'VA012': './Areas/VA012/Contents/src/VA012css.css'
    },
    output: {
        filename: ({ chunk }) => {
            const name = chunk.name;
            const version = versions[name] || '1.0.0.0'; // Default version if not specified
            return `${name}.min.js`;
        },
        path: path.resolve(__dirname, 'Areas/VA012/Scripts/dist')
    },
    resolve: {
        extensions: ['.js'],
    },
    module: {
        rules: [
            {
                test: /\.(js|jsx)$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader'
                },
            },
            {
                test: /\.(css|sass)$/,
                use: [MiniCssExtractPlugin.loader, {
                    loader: 'css-loader',
                    options: {
                        url: false,
                    }
                }
                ],
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin(),
        new MiniCssExtractPlugin({
            filename: ({ chunk }) => {
                const name = chunk.name;
                const version = versions[name] || '1.0.0'; // Default version if not specified
                return `../../Contents/${name}.all.min.css`;
            },
        }),
    ],
    optimization: {
        minimize: true, // Webpack 5 uses "minimize" instead of "minimizer"
        minimizer: [
            new CssMinimizerPlugin(), // Minify CSS
            new TerserPlugin({
                terserOptions: {
                    format: {
                        comments: false,
                    },
                },
                extractComments: false,
            }), // Minify JavaScript
        ]
    },
};