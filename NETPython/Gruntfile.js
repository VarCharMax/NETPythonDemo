/// <binding BeforeBuild='default' />
module.exports = function (grunt) {
  const configPath = 'Scripts/.venv/pyvenv.cfg';
  const configDic = {};
  let dllVersion = '';
  let src = '';

  function extractPyVersion(pver) {
    let versionBuild = pver.indexOf('.');
    let pyVersion = pver
      .substring(0, pver.indexOf('.', versionBuild + 1))
      .replace('.', '');
    return pyVersion;
  }

  grunt.initConfig({
    project: {
      dll: dllVersion,
      source: src,
    },

    copy: {
      main: {
        files: [
          {
            expand: true,
            cwd: '<%= src %>/',
            src: [
              '**',
              '!*.dll',
              '!*.exe',
              '!*.json',
              '!*.txt',
              '!**/libs/**',
              '!**/Doc/**',
              '!**/include/**',
              '!**/Scripts/**',
              '<%= dll %>',
            ],
            dest: 'Scripts/.venv/Scripts/',
          },
        ],
      },
    },
  });

  grunt.registerTask(
    'getVEConfig',
    'Get Python settings from virtual environment.',
    function () {
      const pathListString = grunt.file.read(configPath, { encoding: 'utf8' });
      let lines = pathListString.split('\n').filter(Boolean);

      for (const line of lines) {
        // Trim leading/trailing whitespace
        const trimmedLine = line.trim();

        // Skip empty lines and comments
        if (
          trimmedLine === '' ||
          trimmedLine.startsWith('#') ||
          trimmedLine.startsWith(';')
        ) {
          continue;
        }

        // Find the index of the first '=' character
        const equalsIndex = trimmedLine.indexOf('=');

        if (equalsIndex > 0) {
          // Extract the key and value
          let key = trimmedLine.substring(0, equalsIndex).trim();
          let value = trimmedLine.substring(equalsIndex + 1).trim();

          // Handle quoted values (simple implementation)
          if (value.startsWith('"') && value.endsWith('"')) {
            value = value.slice(1, -1);
          } else if (value.startsWith("'") && value.endsWith("'")) {
            value = value.slice(1, -1);
          }

          configDic[key] = value;
        }
      }

      src = configDic['home'].replaceAll('\\', '/') + '/';
      dllVersion = `python${extractPyVersion(configDic['version'])}.dll`;

      grunt.config.set('src', src);
      grunt.config.set('dll', dllVersion);
    }
  );

  grunt.loadNpmTasks('grunt-contrib-copy');
  grunt.registerTask('default', ['getVEConfig', 'copy:main']);
};
