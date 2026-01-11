/// <binding BeforeBuild='default' />
module.exports = function (grunt) {
  let dest = 'Scripts/.venv/Scripts';
  const configPath = 'Scripts/.venv/pyvenv.cfg';
  const configDic = {};

  function extractPyVersion(pver) {
    let versionBuild = pver.indexOf('.');
    let pyVersion = pver
      .substring(0, pver.indexOf('.', versionBuild + 1))
      .replace('.', '');
    return pyVersion;
  }

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

          // Optional: handle quoted values (simple implementation)
          if (value.startsWith('"') && value.endsWith('"')) {
            value = value.slice(1, -1);
          } else if (value.startsWith("'") && value.endsWith("'")) {
            value = value.slice(1, -1);
          }

          // Add the key-value pair to the dictionary
          configDic[key] = value;
        }
      }
      console.log(`dict = ${configDic['version']}`);
      console.log(`version = ${extractPyVersion(configDic['version'])}`);
    }
  );

  // Load the plugin that provides the "copy" task.
  // grunt.loadNpmTasks('grunt-contrib-copy');

  // Define default task that runs the operation(s)
  grunt.registerTask('default', ['getVEConfig']);
};
