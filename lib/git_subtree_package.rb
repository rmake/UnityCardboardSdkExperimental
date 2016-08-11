require "json"
require "fileutils"
require "optparse"
require "open3"

class GitSubtreePackage

  PACKAGE_JSON = "git_subtree_package.json"

  def puts_flush *args
    puts *args
    STDOUT.flush
  end

  def call_system cmd
    puts_flush "cmd #{cmd}"
    unless system cmd
      raise "failed! #{cmd}"
    end
  end

  def self.run


    package = GitSubtreePackage.new

    mode = nil

    case ARGV[0]
    when "init"
      package.init
    when "split"
      package.split ARGV[1..-1]
    end

  end

  def init
    o = {
      "packages" => {},
    }

    self.write_json o
  end

  def read_json
    JSON.parse File.read(PACKAGE_JSON)
  end

  def write_json(o)
    File.write PACKAGE_JSON, JSON.pretty_generate(o)
  end

  def cd_to_root
    while !File.exist?(PACKAGE_JSON)
      c = Dir.pwd
      Dir.chdir ".."
      break if c != Dir.pwd
    end
  end

  def split(args)

    # ruby git_subtree_package/lib/git_subtree_package.rb split test_package tmp/test_package

    puts_flush "args #{args.inspect}"

    sub_path = args[0]
    repos_path = args[1]

    here = Dir.pwd

    puts_flush "here #{here}"

    FileUtils.mkdir_p repos_path
    Dir.chdir repos_path
    repos_absolute_path = Dir.pwd

    puts_flush "repos_absolute_path #{repos_absolute_path}"

    self.call_system "git init --bare"

    Dir.chdir here
    self.cd_to_root
    root = Dir.pwd

    puts_flush "root #{root}"

    o = self.read_json


    Dir.chdir here
    Dir.chdir sub_path
    sub_absolute_path = Dir.pwd

    lib_path = Pathname.new(root).relative_path_from(sub_absolute_path).to_s
    repos_url = Pathname.new(root).relative_path_from(repos_absolute_path).to_s

    puts_flush "lib_path #{lib_path}"
    puts_flush "repos_url #{repos_url}"

    self.call_system "git subtree push --prefix=#{lib_path} #{repos_url} master"
    self.call_system "git subtree split --prefix=#{lib_path} --rejoin"

  end

end

if $0 == __FILE__ then
  GitSubtreePackage.run
end

