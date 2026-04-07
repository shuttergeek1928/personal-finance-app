"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { userService, UserTransferObject, RegisterUserRequest } from "../../services/user";
import { AlertCircle, UserPlus, Search, ShieldCheck, Trash2, Eye, Wallet, CheckCircle, ArrowLeftRight } from "lucide-react";
import Link from "next/link";

export default function UsersPage() {
  const [users, setUsers] = useState<UserTransferObject[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Search state
  const [searchQuery, setSearchQuery] = useState("");

  // Create User modal
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [createData, setCreateData] = useState({
    firstName: "",
    lastName: "",
    userName: "",
    email: "",
    phoneNumber: "",
    password: "",
    confirmPassword: "",
    acceptTerms: true,
  });

  const fetchUsers = async () => {
    setLoading(true);
    setError(null);
    try {
      const query = searchQuery.trim();
      if (query) {
        const isGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(query);
        const res = isGuid 
          ? await userService.getUserById(query)
          : await userService.getUserByEmail(query);
          
        if (res.success && res.data) {
          setUsers([res.data]);
        } else {
          setUsers([]);
        }
      } else {
        const res = await userService.getUsers(1, 50);
        if (res.success && res.data && res.data.items) {
          setUsers(res.data.items);
        } else {
          setUsers([]);
        }
      }
    } catch (err: any) {
      console.error(err);
      setError(err.message || "Failed to fetch users");
      setUsers([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchUsers();
  };

  const clearSearch = () => {
    setSearchQuery("");
    // Wait for state to settle then fetch all
    setTimeout(() => {
        userService.getUsers(1, 50).then(res => {
            if (res.success && res.data && res.data.items) {
                setUsers(res.data.items);
              } else {
                setUsers([]);
              }
        });
    }, 0);
  };

  const handleCreateUser = async () => {
    try {
      const payload: RegisterUserRequest = {
        firstName: createData.firstName,
        lastName: createData.lastName,
        userName: createData.userName,
        email: createData.email,
        phoneNumber: createData.phoneNumber,
        password: createData.password,
        confirmPassword: createData.confirmPassword,
        acceptTerms: createData.acceptTerms,
      };
      const res = await userService.registerUser(payload);
      if (res.success) {
        setIsCreateOpen(false);
        setCreateData({ firstName: "", lastName: "", userName: "", email: "", phoneNumber: "", password: "", confirmPassword: "", acceptTerms: true });
        fetchUsers();
      } else {
        if (res.errors && res.errors.length > 0) {
          alert(res.errors[0]);
        } else {
          alert(res.message || "Failed to create user");
        }
      }
    } catch (err: any) {
      const data = err.response?.data;
      if (data?.errors && data.errors.length > 0) {
        alert(data.errors[0]);
      } else {
        alert(data?.message || err.message || "Failed to create user");
      }
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to soft delete this user?")) return;
    try {
      const res = await userService.deleteUser(id);
      if (res.success) {
        fetchUsers();
      } else {
        alert(res.message || "Failed to delete user");
      }
    } catch (err: any) {
      alert(err.message || "Failed to delete user");
    }
  };

  const handleConfirmEmail = async (id: string) => {
    try {
      const res = await userService.confirmEmail(id);
      if (res.success) {
        fetchUsers();
      } else {
        alert(res.message || "Failed to confirm email");
      }
    } catch (err: any) {
      alert(err.message || "Failed to confirm email");
    }
  };

  if (loading && users.length === 0) {
    return <div className="p-8 text-center text-muted-foreground">Loading users...</div>;
  }

  return (
    <div className="p-8 max-w-6xl mx-auto space-y-8">
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">User Management</h1>
          <p className="text-muted-foreground mt-1">Admin access to manage all system users</p>
        </div>
        
        <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2"><UserPlus className="w-4 h-4" /> Register New User</Button>
          </DialogTrigger>
          <DialogContent className="max-w-md">
            <DialogHeader>
              <DialogTitle>Register User</DialogTitle>
              <DialogDescription>Add a new user to the platform.</DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4 max-h-[60vh] overflow-y-auto">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>First Name</Label>
                  <Input value={createData.firstName} onChange={e => setCreateData({ ...createData, firstName: e.target.value })} />
                </div>
                <div className="space-y-2">
                  <Label>Last Name</Label>
                  <Input value={createData.lastName} onChange={e => setCreateData({ ...createData, lastName: e.target.value })} />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Email</Label>
                  <Input type="email" value={createData.email} onChange={e => setCreateData({ ...createData, email: e.target.value })} />
                </div>
                <div className="space-y-2">
                  <Label>Username</Label>
                  <Input value={createData.userName} onChange={e => setCreateData({ ...createData, userName: e.target.value })} />
                </div>
              </div>
              <div className="space-y-2">
                <Label>Phone Number</Label>
                <Input value={createData.phoneNumber} onChange={e => setCreateData({ ...createData, phoneNumber: e.target.value })} />
              </div>
              <div className="space-y-2">
                <Label>Password</Label>
                <Input type="password" value={createData.password} onChange={e => setCreateData({ ...createData, password: e.target.value })} />
              </div>
              <div className="space-y-2">
                <Label>Confirm Password</Label>
                <Input type="password" value={createData.confirmPassword} onChange={e => setCreateData({ ...createData, confirmPassword: e.target.value })} />
              </div>
              <div className="flex items-center gap-2 pt-2 cursor-pointer" onClick={() => setCreateData({ ...createData, acceptTerms: !createData.acceptTerms })}>
                <input type="checkbox" checked={createData.acceptTerms} readOnly />
                <Label className="text-sm font-normal cursor-pointer">Accept Terms and Conditions</Label>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsCreateOpen(false)}>Cancel</Button>
              <Button onClick={handleCreateUser}>Register</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <div className="bg-card border rounded-lg p-4 flex flex-wrap items-center justify-between gap-4 shadow-sm">
        <form onSubmit={handleSearch} className="flex-1 flex gap-2 w-full">
          <Input 
            placeholder="Search user by email or ID..." 
            value={searchQuery} 
            onChange={e => setSearchQuery(e.target.value)}
            className="max-w-md bg-background"
          />
          <Button type="submit" variant="secondary" className="gap-2"><Search className="w-4 h-4" /> Search</Button>
          {searchQuery && (
             <Button type="button" variant="ghost" onClick={clearSearch}>Clear</Button>
          )}
        </form>
      </div>

      {error && (
        <div className="bg-destructive/10 text-destructive p-4 rounded-md flex items-center gap-2 shadow-sm">
          <AlertCircle className="w-5 h-5" />
          {error}
        </div>
      )}

      {users.length === 0 && !loading && !error && (
        <Card className="border-dashed bg-muted/50 p-12 text-center text-muted-foreground shadow-sm">
          <UserPlus className="w-12 h-12 mx-auto mb-4 opacity-50" />
          <p>No users found in the system.</p>
        </Card>
      )}

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {users.map(user => (
          <Card key={user.id} className="flex flex-col relative overflow-hidden shadow-sm hover:shadow-md transition-shadow">
            {!user.isActive && (
              <div className="absolute top-0 right-0 bg-red-100 text-red-700 text-xs px-3 py-1 font-semibold rounded-bl-lg">
                Deleted / Inactive
              </div>
            )}
            {user.isActive && user.isEmailConfirmed && (
               <div className="absolute top-0 right-0 bg-green-100 text-green-700 text-xs px-3 py-1 font-semibold rounded-bl-lg">
                Active
              </div>
            )}
            
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                {user.fullName || user.userName || user.email?.split('@')[0] || "Unnamed User"}
                {user.isEmailConfirmed && <span title="Email Confirmed" className="flex items-center"><ShieldCheck className="w-4 h-4 text-blue-500" /></span>}
              </CardTitle>
              <CardDescription>
                <div className="flex flex-col gap-1 mt-1">
                  <span>{user.email || "No email"}</span>
                  <span className="text-xs font-mono">{user.id}</span>
                </div>
              </CardDescription>
            </CardHeader>
            <CardContent className="flex-1 text-sm space-y-2">
              <div className="grid grid-cols-2 gap-x-4 gap-y-2 text-muted-foreground">
                <span className="font-semibold text-foreground">Phone:</span>
                <span>{user.phoneNumber || "-"}</span>
                <span className="font-semibold text-foreground">Member Since:</span>
                <span>{new Date(user.createdAt).toLocaleDateString()}</span>
                <span className="font-semibold text-foreground">Status:</span>
                <span className={user.isEmailConfirmed ? "text-green-600 font-medium" : "text-amber-500 font-medium"}>
                  {user.isEmailConfirmed ? "Confirmed" : "Pending Confirmation"}
                </span>
              </div>
            </CardContent>
            <CardFooter className="bg-muted/30 pt-4 flex flex-col gap-2">
              <div className="w-full flex gap-2">
                <Link href={`/users/${user.id}`} passHref className="flex-1">
                  <Button variant="outline" size="sm" className="w-full gap-2">
                    <Eye className="w-4 h-4" /> Profile
                  </Button>
                </Link>
                <Link href={`/users/${user.id}/accounts`} passHref className="flex-1">
                  <Button variant="default" size="sm" className="w-full gap-2">
                    <Wallet className="w-4 h-4" /> Accounts
                  </Button>
                </Link>
              </div>

              <div className="w-full flex gap-2">
                <Link href={`/transactions?userId=${user.id}`} passHref className="w-full">
                  <Button variant="outline" size="sm" className="w-full gap-2 border-indigo-200 text-indigo-700 hover:bg-indigo-50 dark:border-indigo-900/50 dark:text-indigo-400">
                    <ArrowLeftRight className="w-4 h-4" /> View Transactions
                  </Button>
                </Link>
              </div>
              
              <div className="w-full flex gap-2">
                {!user.isEmailConfirmed && user.isActive && (
                  <Button variant="outline" size="sm" className="flex-1 gap-2" onClick={() => handleConfirmEmail(user.id)}>
                    <CheckCircle className="w-4 h-4" /> Confirm Email
                  </Button>
                )}
                {user.isActive && (
                  <Button variant="destructive" size="sm" className={user.isEmailConfirmed ? "w-full gap-2" : "flex-1 gap-2"} onClick={() => handleDelete(user.id)}>
                    <Trash2 className="w-4 h-4" /> Delete
                  </Button>
                )}
              </div>
            </CardFooter>
          </Card>
        ))}
      </div>
    </div>
  );
}
