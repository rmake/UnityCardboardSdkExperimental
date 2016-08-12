require_relative "../lib/git_subtree_package"

RSpec.describe GitSubtreePackage::Manager do

  include GitSubtreePackage::Utility

  it "message return hello" do
    start_dir = Dir.pwd
    puts_flush "pwd #{start_dir}"

    tmp_path = "tmp"
    FileUtils.rm_rf tmp_path
    FileUtils.mkdir_p tmp_path
    Dir.chdir tmp_path
    tmp_dir = Dir.pwd

    test_repos_path = "test_repos"
    FileUtils.mkdir_p test_repos_path
    Dir.chdir test_repos_path
    test_repos_dir = Dir.pwd
    call_system "git init --bare"

    Dir.chdir tmp_dir
    test_cloned_path = "test_cloned"
    call_system "git clone #{test_repos_path} #{test_cloned_path}"
    Dir.chdir test_cloned_path
    test_cloned_dir = Dir.pwd

    File.write "test.txt", "test 12345"
    call_system "git add ."
    call_system "git commit -m 'initial commit'"
    call_system "git push origin master"
    call_system "git subtree add --prefix=git_subtree_package #{GitSubtreePackage::Manager::PACKAGE_REPOS} master"
    call_system "ruby git_subtree_package/lib/git_subtree_package.rb init"
    call_system "ruby git_subtree_package/lib/git_subtree_package.rb add --github #{GitSubtreePackage::Manager::PACKAGE_REPOS} master"
    call_system "git add ."
    call_system "git commit -m 'add: git_subtree_package'"

    expect(Dir[GitSubtreePackage::Manager::PACKAGE_JSON].length).to eq 1

    module_repos_path = "module_repos"
    module_path = "module"
    FileUtils.mkdir_p module_path
    File.write "module/module.txt", "module 1\n"

    call_system "git add ."
    call_system "git commit -m 'add: module 1'"
    call_system "git push origin master"

    call_system "ruby git_subtree_package/lib/git_subtree_package.rb split module ../#{module_repos_path}"

    File.write "module/module.txt", File.read("module/module.txt") + "module 2\n"

    call_system "git add ."
    call_system "git commit -m 'add: module 2'"
    call_system "git push origin master"

    call_system "ruby git_subtree_package/lib/git_subtree_package.rb push module master"

    Dir.chdir tmp_dir
    module_cloned_path = "module_cloned"
    call_system "git clone #{module_repos_path} #{module_cloned_path}"
    Dir.chdir module_cloned_path
    module_cloned_dir = Dir.pwd

    Dir.chdir module_cloned_dir

    expect(File.read("module.txt")).to eq <<EOS
module 1
module 2
EOS

    File.write "module.txt", File.read("module.txt") + "module 3\n"

    call_system "git add ."
    call_system "git commit -m 'add: module 3'"
    call_system "git push origin master"

    Dir.chdir test_cloned_dir

    call_system "ruby git_subtree_package/lib/git_subtree_package.rb pull module master"
    call_system "git push origin master"
    expect(File.read("module/module.txt")).to eq <<EOS
module 1
module 2
module 3
EOS


    #call_system "git lga -n 20"

    Dir.chdir start_dir
    #FileUtils.rm_rf tmp_dir
    expect("test").to eq "test"
  end
end

